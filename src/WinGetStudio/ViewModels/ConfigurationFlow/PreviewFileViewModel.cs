// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Windows.Storage;
using WinGetStudio.Exceptions;
using WinGetStudio.Models;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Exceptions;
using WinGetStudio.Services.DesiredStateConfiguration.Models;
using WingetStudio.Services.VisualFeedback.Contracts;
using WingetStudio.Services.VisualFeedback.Models;

namespace WinGetStudio.ViewModels.ConfigurationFlow;

public partial class PreviewFileViewModel : ObservableRecipient
{
    private readonly ILogger<PreviewFileViewModel> _logger;
    private readonly IStringLocalizer<PreviewFileViewModel> _localizer;
    private readonly IUIFeedbackService _ui;
    private readonly IDSC _dsc;

    public IReadOnlyList<UnitSecurityContext> SecurityContexts => UnitSecurityContext.All;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmptyState))]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmptyState))]
    [NotifyPropertyChangedFor(nameof(CanApplyConfiguration))]
    [NotifyPropertyChangedFor(nameof(CanValidateConfiguration))]
    [NotifyPropertyChangedFor(nameof(CanSaveConfiguration))]
    [NotifyCanExecuteChangedFor(nameof(AddResourceCommand))]
    [NotifyCanExecuteChangedFor(nameof(ApplyConfigurationCommand))]
    [NotifyCanExecuteChangedFor(nameof(ValidateConfigurationCommand))]
    [NotifyCanExecuteChangedFor(nameof(ToggleEditModeCommand))]
    public partial DSCSetViewModel ConfigurationSet { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsUnitSelected))]
    public partial Tuple<DSCUnitViewModel, DSCUnitViewModel>? SelectedUnit { get; set; }

    [ObservableProperty]
    public partial bool IsEditMode { get; set; }

    [ObservableProperty]
    public partial bool IsCodeView { get; set; }

    public bool IsUnitSelected => SelectedUnit != null;

    public bool IsEmptyState => !IsLoading && ConfigurationSet == null;

    public bool CanAddResource => ConfigurationSet != null;

    public bool CanToggleEditMode => ConfigurationSet != null;

    public bool CanApplyConfiguration => ConfigurationSet?.Units.Count > 0;

    public bool CanValidateConfiguration => ConfigurationSet?.Units.Count > 0;

    public bool CanSaveConfiguration => ConfigurationSet?.Units.Count > 0;

    public PreviewFileViewModel(
        ILogger<PreviewFileViewModel> logger,
        IStringLocalizer<PreviewFileViewModel> localizer,
        IUIFeedbackService ui,
        IDSC dsc)
    {
        _logger = logger;
        _localizer = localizer;
        _ui = ui;
        _dsc = dsc;
    }

    /// <summary>
    /// Opens the configuration file.
    /// </summary>
    /// <param name="file">The configuration file to open.</param>
    public async Task OpenConfigurationFileAsync(StorageFile file)
    {
        try
        {
            _ui.ShowTaskProgress();
            _logger.LogInformation($"Selected file: {file.Path}");
            IsEditMode = false;
            IsLoading = true;
            SelectedUnit = null;
            ConfigurationSet = new DSCSetViewModel(_logger);
            var dscFile = await DSCFile.LoadAsync(file.Path);
            var dscSet = await _dsc.OpenConfigurationSetAsync(dscFile);
            ConfigurationSet.Use(dscSet, dscFile);
            _dsc.GetConfigurationUnitDetails(dscSet);
        }
        catch (OpenConfigurationSetException ex)
        {
            _logger.LogError(ex, $"Opening configuration set failed");
            _ui.ShowTimedNotification(ex.GetErrorMessage(_localizer), NotificationMessageSeverity.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unknown error while opening configuration set");
            _ui.ShowTimedNotification(ex.Message, NotificationMessageSeverity.Error);
        }
        finally
        {
            IsLoading = false;
            _ui.HideTaskProgress();
        }
    }

    [RelayCommand]
    private async Task OnNewConfigurationAsync()
    {
        try
        {
            _ui.ShowTaskProgress();
            _logger.LogInformation($"Creating new configuration set");
            SelectedUnit = null;
            ConfigurationSet = new DSCSetViewModel(_logger);
            await AddResourceAsync();
        }
        catch (DSCUnitValidationException ex)
        {
            _logger.LogError(ex, "Validation of the new configuration unit failed");
            _ui.ShowTimedNotification(ex.Message, NotificationMessageSeverity.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Creating new configuration set failed");
            _ui.ShowTimedNotification($"Creating new configuration set failed: {ex.Message}", NotificationMessageSeverity.Error);
        }
        finally
        {
            _ui.HideTaskProgress();
        }
    }

    [RelayCommand(CanExecute = nameof(CanSaveConfiguration))]
    private async Task OnSaveConfigurationAsync()
    {
        await Task.CompletedTask;
    }

    [RelayCommand(CanExecute = nameof(CanSaveConfiguration))]
    private async Task OnSaveConfigurationAsAsync()
    {
        await Task.CompletedTask;
    }

    [RelayCommand]
    private void OnValidateUnit(DSCUnitViewModel unit)
    {
        // TODO
    }

    [RelayCommand]
    private void OnEditUnit(DSCUnitViewModel unit)
    {
        _logger.LogInformation($"Editing unit {unit.Title}");
        EditUnit(unit);
    }

    [RelayCommand]
    private async Task OnDeleteSelectedUnitAsync()
    {
        if (ConfigurationSet != null && SelectedUnit != null)
        {
            try
            {
                _ui.ShowTaskProgress();
                _logger.LogInformation($"Deleting selected unit {SelectedUnit.Item1.Title}");
                await ConfigurationSet.RemoveAsync(SelectedUnit.Item1);
                SelectedUnit = null;
                _ui.ShowTimedNotification($"Configuration unit deleted", NotificationMessageSeverity.Success);
            }
            catch (DSCUnitValidationException ex)
            {
                _logger.LogError(ex, "Validation of configuration units failed after deletion");
                _ui.ShowTimedNotification(ex.Message, NotificationMessageSeverity.Error);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Deleting configuration unit failed");
                _ui.ShowTimedNotification($"Deleting configuration unit failed: {ex.Message}", NotificationMessageSeverity.Error);
            }
            finally
            {
                _ui.HideTaskProgress();
            }
        }
    }

    [RelayCommand(CanExecute = nameof(CanAddResource))]
    private async Task OnAddResourceAsync()
    {
        try
        {
            _ui.ShowTaskProgress();
            _logger.LogInformation($"Adding new resource");
            await AddResourceAsync();
        }
        catch (DSCUnitValidationException ex)
        {
            _logger.LogError(ex, "Validation of the added configuration unit failed");
            _ui.ShowTimedNotification(ex.Message, NotificationMessageSeverity.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Adding new resource failed");
            _ui.ShowTimedNotification($"Adding new resource failed: {ex.Message}", NotificationMessageSeverity.Error);
        }
        finally
        {
            _ui.HideTaskProgress();
        }
    }

    [RelayCommand(CanExecute = nameof(CanValidateConfiguration))]
    private async Task OnValidateConfigurationAsync()
    {
        if (ConfigurationSet != null)
        {
            try
            {
                _ui.ShowTaskProgress();
                _logger.LogInformation($"Validating configuration code");
                var dscFile = ConfigurationSet.GetLatestDSCFile();
                var dscSet = await _dsc.OpenConfigurationSetAsync(dscFile);
                await _dsc.ValidateSetAsync(dscSet);
                _ui.ShowTimedNotification($"Configuration code is valid", NotificationMessageSeverity.Success);
            }
            catch (OpenConfigurationSetException ex)
            {
                _logger.LogError(ex, $"Opening configuration set failed during validation");
                _ui.ShowTimedNotification(ex.GetErrorMessage(_localizer), NotificationMessageSeverity.Error);
            }
            catch (ApplyConfigurationSetException ex)
            {
                _logger.LogError(ex, $"Validation of configuration set failed");
                _ui.ShowTimedNotification(ex.GetSetErrorMessage(_localizer), NotificationMessageSeverity.Error);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unknown error while validating configuration code");
                _ui.ShowTimedNotification(ex.Message, NotificationMessageSeverity.Error);
            }
            finally
            {
                _ui.HideTaskProgress();
            }
        }
    }

    [RelayCommand(CanExecute = nameof(CanApplyConfiguration))]
    private async Task OnApplyConfigurationAsync()
    {
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task OnUpdateSelectedUnitAsync()
    {
        if (ConfigurationSet != null && SelectedUnit != null)
        {
            try
            {
                _ui.ShowTaskProgress();
                _logger.LogInformation($"Updating selected unit {SelectedUnit.Item1.Title}");
                await ConfigurationSet.UpdateAsync(SelectedUnit.Item1, SelectedUnit.Item2);
                _ui.ShowTimedNotification($"Configuration unit updated", NotificationMessageSeverity.Success);
            }
            catch (DSCUnitValidationException ex)
            {
                _logger.LogError(ex, "Validation of configuration unit failed");
                _ui.ShowTimedNotification(ex.Message, NotificationMessageSeverity.Error);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Updating configuration unit failed");
                _ui.ShowTimedNotification($"Updating configuration unit failed: {ex.Message}", NotificationMessageSeverity.Error);
            }
            finally
            {
                _ui.HideTaskProgress();
            }
        }
    }

    [RelayCommand]
    private void OnCancelSelectedUnit()
    {
        _logger.LogInformation("Cancelling edit of selected unit");
        SelectedUnit = null;
    }

    [RelayCommand(CanExecute = nameof(CanToggleEditMode))]
    private void OnToggleEditMode()
    {
        _logger.LogInformation($"Toggling edit mode to {(IsEditMode ? "ON" : "OFF")}");
    }

    [RelayCommand]
    private void OnToggleCodeView()
    {
        _logger.LogInformation($"Toggling code view to {(IsCodeView ? "ON" : "OFF")}");
    }

    private void EditUnit(DSCUnitViewModel unit)
    {
        IsEditMode = true;
        SelectedUnit = new(unit, unit.Clone());
    }

    private async Task AddResourceAsync()
    {
        if (ConfigurationSet != null)
        {
            var unit = new DSCUnitViewModel() { Title = "Module/Resource" };
            await ConfigurationSet.AddAsync(unit);
            EditUnit(unit);
        }
    }

    partial void OnConfigurationSetChanged(DSCSetViewModel? oldValue, DSCSetViewModel? newValue)
    {
        oldValue?.UnitsCollectionChanged -= OnConfigurationSetUnitsChanged;
        newValue?.UnitsCollectionChanged += OnConfigurationSetUnitsChanged;
    }

    private void OnConfigurationSetUnitsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Notify validation
        OnPropertyChanged(nameof(CanValidateConfiguration));
        ValidateConfigurationCommand.NotifyCanExecuteChanged();

        // Notify apply
        OnPropertyChanged(nameof(CanApplyConfiguration));
        ApplyConfigurationCommand.NotifyCanExecuteChanged();

        // Notify save
        OnPropertyChanged(nameof(CanSaveConfiguration));
        SaveConfigurationCommand.NotifyCanExecuteChanged();
        SaveConfigurationAsCommand.NotifyCanExecuteChanged();
    }
}

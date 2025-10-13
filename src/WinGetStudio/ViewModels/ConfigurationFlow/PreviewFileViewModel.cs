// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.ObjectModel;
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
using WinGetStudio.Services.DesiredStateConfiguration.Extensions;
using WinGetStudio.Services.DesiredStateConfiguration.Models;
using WinGetStudio.Services.DesiredStateConfiguration.Models.Schemas.ConfigurationV3;
using WingetStudio.Services.VisualFeedback.Contracts;
using WingetStudio.Services.VisualFeedback.Models;

namespace WinGetStudio.ViewModels.ConfigurationFlow;

public partial class PreviewFileViewModel : ObservableRecipient
{
    private readonly ILogger<PreviewFileViewModel> _logger;
    private readonly IStringLocalizer<PreviewFileViewModel> _localizer;
    private readonly IUIFeedbackService _ui;
    private readonly IDSC _dsc;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmptyState))]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmptyState))]
    [NotifyCanExecuteChangedFor(nameof(AddResourceCommand))]
    [NotifyCanExecuteChangedFor(nameof(ApplyConfigurationCommand))]
    [NotifyCanExecuteChangedFor(nameof(ValidateConfigurationCommand))]
    public partial ObservableCollection<DSCUnitViewModel>? ConfigurationUnits { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsUnitSelected))]
    public partial Tuple<DSCUnitViewModel, DSCUnitViewModel>? SelectedUnit { get; set; }

    [ObservableProperty]
    public partial string? ConfigurationCode { get; set; }

    public bool IsUnitSelected => SelectedUnit != null;

    [ObservableProperty]
    public partial bool IsEditMode { get; set; }

    public bool IsEmptyState => !IsLoading && ConfigurationUnits == null;

    public IReadOnlyList<UnitSecurityContext> SecurityContexts => UnitSecurityContext.All;

    public bool CanAddResource => ConfigurationUnits != null;

    public bool CanApplyConfiguration => ConfigurationUnits?.Count > 0;

    public bool CanValidateConfiguration => ConfigurationUnits?.Count > 0;

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
            _logger.LogInformation($"Selected file: {file.Path}");
            ClearConfigurationSet();
            IsEditMode = false;
            IsLoading = true;
            var dscFile = await DSCFile.LoadAsync(file.Path);
            var dscSet = await _dsc.OpenConfigurationSetAsync(dscFile);
            _dsc.GetConfigurationUnitDetails(dscSet);
            ShowConfigurationSet(dscSet, dscFile.Content);
            ResolveDependencies();
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
        }
    }

    [RelayCommand]
    private void OnNewConfiguration()
    {
        _logger.LogInformation($"Creating new configuration set");
        ClearConfigurationSet();
        ConfigurationUnits = [];
        AddResource();
    }

    [RelayCommand]
    private async Task OnSaveAsync()
    {
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task OnSaveAsAsync()
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
        EditUnit(unit);
    }

    [RelayCommand]
    private void OnDeleteUnit(DSCUnitViewModel unit)
    {
        if (SelectedUnit?.Item1 == unit)
        {
            SelectedUnit = null;
        }

        ConfigurationUnits?.Remove(unit);
    }

    [RelayCommand(CanExecute = nameof(CanAddResource))]
    private void OnAddResource()
    {
        AddResource();
    }

    [RelayCommand(CanExecute = nameof(CanValidateConfiguration))]
    private async Task OnValidateConfigurationAsync()
    {
        await Task.CompletedTask;
    }

    [RelayCommand(CanExecute = nameof(CanApplyConfiguration))]
    private async Task OnApplyConfigurationAsync()
    {
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task OnUpdateSelectedUnitAsync()
    {
        if (SelectedUnit != null)
        {
            try
            {
                _ui.ShowTaskProgress();
                SelectedUnit.Item2.Validate();
                SelectedUnit.Item1.CopyFrom(SelectedUnit.Item2);
                await UpdateConfigurationCodeAsync();
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
        SelectedUnit = null;
    }

    [RelayCommand]
    private async Task OnToggleViewModeAsync()
    {
        // The value of IsEditMode will be toggled after this method returns.
        var wasEditMode = IsEditMode;
        var isSwitchingToEditMode = !wasEditMode;
        if (isSwitchingToEditMode)
        {
            try
            {
                _ui.ShowTaskProgress();
                await UpdateConfigurationCodeAsync();
            }
            catch (DSCUnitValidationException ex)
            {
                _logger.LogError(ex, "Validation of configuration unit failed");
                _ui.ShowTimedNotification(ex.Message, NotificationMessageSeverity.Error);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Generating configuration code failed");
                _ui.ShowTimedNotification($"Generating configuration code failed: {ex.Message}", NotificationMessageSeverity.Error);
            }
            finally
            {
                _ui.HideTaskProgress();
            }
        }
    }

    /// <summary>
    /// Shows the configuration set.
    /// </summary>
    /// <param name="dscSet">The configuration set to show.</param>
    private void ShowConfigurationSet(IDSCSet? dscSet, string? dscCode)
    {
        ConfigurationUnits = dscSet == null ? null : new(dscSet.Units.Select(unit => new DSCUnitViewModel(unit)));
        ConfigurationCode = dscCode;
        SelectedUnit = default;
    }

    /// <summary>
    /// Clears the current configuration set.
    /// </summary>
    private void ClearConfigurationSet() => ShowConfigurationSet(null, null);

    private void ResolveDependencies()
    {
        var configurationUnits = ConfigurationUnits ?? [];
        foreach (var unit in configurationUnits)
        {
            unit.ResolveDependencies(configurationUnits);
        }
    }

    private async Task UpdateConfigurationCodeAsync()
    {
        ConfigurationCode = await GenerateConfigurationCodeAsync();
    }

    private Task<string> GenerateConfigurationCodeAsync()
    {
        return Task.Run(() =>
        {
            if (ConfigurationUnits == null)
            {
                return string.Empty;
            }

            var config = new ConfigurationV3();
            config.AddWinGetMetadata();
            foreach (var unit in ConfigurationUnits)
            {
                var unitConfig = unit.ToConfigurationV3();
                config.Resources.AddRange(unitConfig.Resources);
            }

            return config.ToYaml();
        });
    }

    private void EditUnit(DSCUnitViewModel unit)
    {
        IsEditMode = true;
        SelectedUnit = new(unit, unit.Clone());
    }

    private void AddResource()
    {
        if (ConfigurationUnits != null)
        {
            var unit = new DSCUnitViewModel()
            {
                Title = "Module/Resource",
                SelectedSecurityContext = UnitSecurityContext.Default,
            };
            ConfigurationUnits.Insert(0, unit);
            EditUnit(unit);
        }
    }

    partial void OnConfigurationUnitsChanged(ObservableCollection<DSCUnitViewModel>? oldValue, ObservableCollection<DSCUnitViewModel>? newValue)
    {
        oldValue?.CollectionChanged -= OnConfigurationUnitsContentChanged;
        newValue?.CollectionChanged += OnConfigurationUnitsContentChanged;
    }

    private void OnConfigurationUnitsContentChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(CanValidateConfiguration));
        OnPropertyChanged(nameof(CanApplyConfiguration));
        ValidateConfigurationCommand.NotifyCanExecuteChanged();
        ApplyConfigurationCommand.NotifyCanExecuteChanged();
    }
}

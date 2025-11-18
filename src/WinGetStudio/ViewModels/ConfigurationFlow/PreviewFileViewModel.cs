// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Management.Configuration;
using Windows.Storage;
using WinGetStudio.Contracts.Services;
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
    private readonly IAppFrameNavigationService _appNavigation;
    private readonly IConfigurationFrameNavigationService _configNavigation;
    private readonly IConfigurationManager _manager;
    private readonly SetViewModelFactory _setFactory;
    private readonly UnitViewModelFactory _unitFactory;

    public IReadOnlyList<UnitSecurityContext> SecurityContexts => UnitSecurityContext.All;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmptyState))]
    [NotifyPropertyChangedFor(nameof(CanApplyConfiguration))]
    [NotifyPropertyChangedFor(nameof(CanApplyConfigurationOrViewResult))]
    [NotifyPropertyChangedFor(nameof(CanValidateConfiguration))]
    [NotifyPropertyChangedFor(nameof(CanTestConfiguration))]
    [NotifyPropertyChangedFor(nameof(CanSaveConfigurationAs))]
    [NotifyPropertyChangedFor(nameof(HasNoUnits))]
    [NotifyCanExecuteChangedFor(nameof(AddResourceCommand))]
    [NotifyCanExecuteChangedFor(nameof(ApplyConfigurationCommand))]
    [NotifyCanExecuteChangedFor(nameof(TestConfigurationCommand))]
    [NotifyCanExecuteChangedFor(nameof(ValidateConfigurationCommand))]
    [NotifyCanExecuteChangedFor(nameof(ToggleEditModeCommand))]
    [NotifyCanExecuteChangedFor(nameof(SaveConfigurationCommand))]
    public partial SetViewModel? ConfigurationSet { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsUnitSelected))]
    public partial Tuple<UnitViewModel, UnitViewModel>? SelectedUnit { get; set; }

    [ObservableProperty]
    public partial bool IsEditMode { get; set; }

    [ObservableProperty]
    public partial bool IsCodeView { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmptyState))]
    public partial bool IsConfigurationLoading { get; set; }

    [MemberNotNullWhen(true, nameof(ConfigurationSet))]
    public bool IsConfigurationLoaded => ConfigurationSet != null;

    public bool IsApplyInProgress => ActiveApplySet != null;

    public bool IsReadOnlyMode => IsApplyInProgress;

    public bool IsUnitSelected => SelectedUnit != null;

    public bool IsEmptyState => !IsConfigurationLoading && !IsConfigurationLoaded;

    public bool CanAddUnit => IsConfigurationLoaded && !IsReadOnlyMode;

    public bool CanUpdateUnit => !IsReadOnlyMode;

    public bool CanDeleteUnit => !IsReadOnlyMode;

    public bool CanToggleEditMode => IsConfigurationLoaded;

    public bool CanApplyConfiguration => ConfigurationSet?.Units.Count > 0 && !IsApplyInProgress;

    public bool CanViewResults => IsApplyInProgress;

    public bool CanApplyConfigurationOrViewResult => CanApplyConfiguration || CanViewResults;

    public bool CanTestConfiguration => ConfigurationSet?.Units.Count > 0 && !IsApplyInProgress;

    public bool CanValidateConfiguration => ConfigurationSet?.Units.Count > 0 && !IsApplyInProgress;

    public bool CanSaveConfiguration => IsConfigurationLoaded && ConfigurationSet.CanSave && !IsReadOnlyMode;

    public bool CanSaveConfigurationAs => IsConfigurationLoaded && !IsReadOnlyMode;

    public bool CanOpenConfigurationFile => !IsApplyInProgress;

    public bool CanCreateNewConfiguration => !IsApplyInProgress;

    public bool HasNoUnits => IsConfigurationLoaded && ConfigurationSet.Units.Count == 0;

    public ApplySetViewModel? ActiveApplySet => _manager.ActiveSetApplyState.ActiveApplySet;

    public PreviewFileViewModel(
        ILogger<PreviewFileViewModel> logger,
        IStringLocalizer<PreviewFileViewModel> localizer,
        IUIFeedbackService ui,
        IDSC dsc,
        IAppFrameNavigationService appNavigation,
        IConfigurationFrameNavigationService configNavigation,
        IConfigurationManager manager,
        UnitViewModelFactory unitFactory,
        SetViewModelFactory setFactory)
    {
        _logger = logger;
        _localizer = localizer;
        _ui = ui;
        _dsc = dsc;
        _appNavigation = appNavigation;
        _configNavigation = configNavigation;
        _manager = manager;
        _unitFactory = unitFactory;
        _setFactory = setFactory;
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
            IsConfigurationLoading = true;
            SelectedUnit = null;
            ConfigurationSet = _setFactory();
            var dscFile = await DSCFile.LoadAsync(file.Path);
            var dscSet = await _dsc.OpenConfigurationSetAsync(dscFile);
            await ConfigurationSet.UseAsync(dscSet, dscFile);
            SaveConfigurationCommand.NotifyCanExecuteChanged();
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
            IsConfigurationLoading = false;
            _ui.HideTaskProgress();
        }
    }

    public async Task SaveConfigurationAsAsync(string filePath)
    {
        if (IsConfigurationLoaded)
        {
            try
            {
                _ui.ShowTaskProgress();
                _logger.LogInformation($"Saving configuration set as {filePath}");
                await ConfigurationSet.SaveAsAsync(filePath);
                _ui.ShowTimedNotification(_localizer["PreviewFile_SavedSuccessfully"], NotificationMessageSeverity.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Saving configuration set failed");
                _ui.ShowTimedNotification(_localizer["PreviewFile_SaveFailed", ex.Message], NotificationMessageSeverity.Error);
            }
            finally
            {
                // After saving as a new file, re-evaluate if saving is possible
                SaveConfigurationCommand.NotifyCanExecuteChanged();
                _ui.HideTaskProgress();
            }
        }
    }

    [RelayCommand]
    private void OnLoaded()
    {
        if (_manager.ActiveSetPreviewState.CanRestoreState())
        {
            _manager.ActiveSetPreviewState.RestoreState(this);
        }
    }

    [RelayCommand]
    private void OnUnloaded()
    {
        _manager.ActiveSetPreviewState.CaptureState(this);
    }

    [RelayCommand(CanExecute = nameof(CanCreateNewConfiguration))]
    private async Task OnNewConfigurationAsync()
    {
        try
        {
            _ui.ShowTaskProgress();
            _logger.LogInformation($"Creating new configuration set");
            SelectedUnit = null;
            ConfigurationSet = _setFactory();
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
            _ui.ShowTimedNotification(_localizer["PreviewFile_CreateNewConfigurationFailed", ex.Message], NotificationMessageSeverity.Error);
        }
        finally
        {
            _ui.HideTaskProgress();
        }
    }

    [RelayCommand(CanExecute = nameof(CanSaveConfiguration))]
    private async Task OnSaveConfigurationAsync()
    {
        if (IsConfigurationLoaded)
        {
            try
            {
                _ui.ShowTaskProgress();
                _logger.LogInformation($"Saving configuration set");
                await ConfigurationSet.SaveAsync();
                _ui.ShowTimedNotification(_localizer["PreviewFile_SaveSuccessfulMessage"], NotificationMessageSeverity.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Saving configuration set failed");
                _ui.ShowTimedNotification(_localizer["PreviewFile_SaveFailedMessage", ex.Message], NotificationMessageSeverity.Error);
            }
            finally
            {
                _ui.HideTaskProgress();
            }
        }
    }

    [RelayCommand]
    private async Task OnValidateUnitAsync(UnitViewModel unit)
    {
        if (IsConfigurationLoaded && unit != null)
        {
            _logger.LogInformation($"Validating unit {unit.Title}");
            var unitClone = await unit.CloneAsync();
            var param = new ValidateUnitNavigationContext(unitClone);
            _appNavigation.NavigateTo<ValidationViewModel>(param);
        }
    }

    [RelayCommand]
    private async Task OnEditUnit(UnitViewModel unit)
    {
        _logger.LogInformation($"Editing unit {unit.Title}");
        await EditUnitAsync(unit);
    }

    [RelayCommand(CanExecute = nameof(CanDeleteUnit))]
    private async Task OnDeleteSelectedUnitAsync()
    {
        if (IsConfigurationLoaded && SelectedUnit != null)
        {
            try
            {
                _ui.ShowTaskProgress();
                _logger.LogInformation($"Deleting selected unit {SelectedUnit.Item1.Title}");
                await ConfigurationSet.RemoveAsync(SelectedUnit.Item1);
                SelectedUnit = null;
                _ui.ShowTimedNotification(_localizer["PreviewFile_DeleteSuccessfulMessage"], NotificationMessageSeverity.Success);
            }
            catch (DSCUnitValidationException ex)
            {
                _logger.LogError(ex, "Validation of configuration units failed after deletion");
                _ui.ShowTimedNotification(ex.Message, NotificationMessageSeverity.Error);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Deleting configuration unit failed");
                _ui.ShowTimedNotification(_localizer["PreviewFile_DeleteFailedMessage", ex.Message], NotificationMessageSeverity.Error);
            }
            finally
            {
                _ui.HideTaskProgress();
            }
        }
    }

    [RelayCommand(CanExecute = nameof(CanAddUnit))]
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
            _ui.ShowTimedNotification(_localizer["PreviewFile_AddResourceFailedMessage", ex.Message], NotificationMessageSeverity.Error);
        }
        finally
        {
            _ui.HideTaskProgress();
        }
    }

    [RelayCommand(CanExecute = nameof(CanValidateConfiguration))]
    private async Task OnValidateConfigurationAsync()
    {
        if (IsConfigurationLoaded)
        {
            try
            {
                _ui.ShowTaskProgress();
                _logger.LogInformation($"Validating configuration code");
                var dscFile = ConfigurationSet.GetLatestDSCFile();
                var dscSet = await _dsc.OpenConfigurationSetAsync(dscFile);
                await _dsc.ValidateSetAsync(dscSet);
                _ui.ShowTimedNotification(_localizer["PreviewFile_ValidationSuccessfulMessage"], NotificationMessageSeverity.Success);
            }
            catch (OpenConfigurationSetException ex)
            {
                _logger.LogError(ex, $"Opening configuration set failed during validation");
                _ui.ShowTimedNotification(ex.GetErrorMessage(_localizer), NotificationMessageSeverity.Error);
            }
            catch (ApplyConfigurationSetException ex)
            {
                _logger.LogError(ex, $"Validation of configuration set failed");
                var title = ex.GetSetErrorMessage(_localizer);
                var message = ex.GetUnitsSummaryMessage(_localizer);
                _ui.ShowTimedNotification(title, message, NotificationMessageSeverity.Error);
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

    [RelayCommand(CanExecute = nameof(CanTestConfiguration))]
    private async Task OnTestConfigurationAsync()
    {
        if (IsConfigurationLoaded)
        {
            try
            {
                _ui.ShowTaskProgress();
                _logger.LogInformation($"Testing configuration code");
                var dscFile = ConfigurationSet.GetLatestDSCFile();
                var dscSet = await _dsc.OpenConfigurationSetAsync(dscFile);
                var result = await _dsc.TestSetAsync(dscSet);
                if (result.TestResult == ConfigurationTestResult.Positive)
                {
                    _ui.ShowTimedNotification(_localizer["Notification_MachineInDesiredState"], NotificationMessageSeverity.Success);
                }
                else
                {
                    _ui.ShowTimedNotification(_localizer["Notification_MachineNotInDesiredState"], NotificationMessageSeverity.Error);
                }
            }
            catch (OpenConfigurationSetException ex)
            {
                _logger.LogError(ex, $"Opening configuration set failed during validation");
                _ui.ShowTimedNotification(ex.GetErrorMessage(_localizer), NotificationMessageSeverity.Error);
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

    [RelayCommand(CanExecute = nameof(CanViewResults))]
    private void OnViewResult()
    {
        if (IsApplyInProgress)
        {
            _configNavigation.NavigateTo<ApplyFileViewModel>();
        }
    }

    [RelayCommand(CanExecute = nameof(CanApplyConfiguration))]
    private void OnApplyConfiguration()
    {
        if (!IsApplyInProgress)
        {
            _manager.ActiveSetPreviewState.CaptureState(this);
            _configNavigation.NavigateTo<ApplyFileViewModel>();
        }
    }

    [RelayCommand(CanExecute = nameof(CanUpdateUnit))]
    private async Task OnUpdateSelectedUnitAsync()
    {
        if (IsConfigurationLoaded && SelectedUnit != null)
        {
            try
            {
                _ui.ShowTaskProgress();
                _logger.LogInformation($"Updating selected unit {SelectedUnit.Item1.Title}");
                await ConfigurationSet.UpdateAsync(SelectedUnit.Item1, SelectedUnit.Item2);
                _ui.ShowTimedNotification(_localizer["PreviewFile_ValidationFailedMessage"], NotificationMessageSeverity.Success);
            }
            catch (DSCUnitValidationException ex)
            {
                _logger.LogError(ex, "Validation of configuration unit failed");
                _ui.ShowTimedNotification(ex.Message, NotificationMessageSeverity.Error);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Updating configuration unit failed");
                _ui.ShowTimedNotification(_localizer["PreviewFile_UpdateFailedMessage", ex.Message], NotificationMessageSeverity.Error);
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

    private async Task EditUnitAsync(UnitViewModel unit)
    {
        IsEditMode = true;
        var unitClone = await unit.CloneAsync();
        SelectedUnit = new(unit, unitClone);
    }

    private async Task AddResourceAsync()
    {
        if (IsConfigurationLoaded)
        {
            var unit = _unitFactory();
            unit.Title = "Module/Resource";
            await ConfigurationSet.AddAsync(unit);
            await EditUnitAsync(unit);
        }
    }

    partial void OnConfigurationSetChanged(SetViewModel? oldValue, SetViewModel? newValue)
    {
        oldValue?.UnitsCollectionChanged -= OnConfigurationSetUnitsChanged;
        newValue?.UnitsCollectionChanged += OnConfigurationSetUnitsChanged;
    }

    private void OnConfigurationSetUnitsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Notify validation
        OnPropertyChanged(nameof(CanValidateConfiguration));
        ValidateConfigurationCommand.NotifyCanExecuteChanged();

        // Notify test
        OnPropertyChanged(nameof(CanTestConfiguration));
        TestConfigurationCommand.NotifyCanExecuteChanged();

        // Notify apply
        OnPropertyChanged(nameof(CanApplyConfiguration));
        OnPropertyChanged(nameof(CanApplyConfigurationOrViewResult));
        ApplyConfigurationCommand.NotifyCanExecuteChanged();

        // Notify save
        SaveConfigurationCommand.NotifyCanExecuteChanged();

        // Notify save as
        OnPropertyChanged(nameof(CanSaveConfigurationAs));

        // Notify HasNoUnits
        OnPropertyChanged(nameof(HasNoUnits));
    }
}

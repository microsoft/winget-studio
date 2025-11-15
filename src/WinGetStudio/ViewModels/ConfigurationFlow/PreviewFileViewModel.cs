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
using WinGetStudio.Services.Operations.Contracts;
using WinGetStudio.Services.Operations.Extensions;

namespace WinGetStudio.ViewModels.ConfigurationFlow;

public partial class PreviewFileViewModel : ObservableRecipient
{
    private readonly ILogger<PreviewFileViewModel> _logger;
    private readonly IStringLocalizer<PreviewFileViewModel> _localizer;
    private readonly IOperationHub _operationHub;
    private readonly IDSCOperationHub _dscOperationHub;
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

    public ApplySetViewModel? ActiveApplySet => _manager.ActiveSetApplyState.ActiveApplySet;

    public PreviewFileViewModel(
        ILogger<PreviewFileViewModel> logger,
        IStringLocalizer<PreviewFileViewModel> localizer,
        IOperationHub operationHub,
        IDSCOperationHub dscOperationHub,
        IDSC dsc,
        IAppFrameNavigationService appNavigation,
        IConfigurationFrameNavigationService configNavigation,
        IConfigurationManager manager,
        UnitViewModelFactory unitFactory,
        SetViewModelFactory setFactory)
    {
        _logger = logger;
        _localizer = localizer;
        _operationHub = operationHub;
        _dscOperationHub = dscOperationHub;
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
        IsEditMode = false;
        IsConfigurationLoading = true;
        SelectedUnit = null;
        ConfigurationSet = _setFactory();
        var dscFile = await DSCFile.LoadAsync(file.Path);
        var operationResult = await _dscOperationHub.ExecuteOpenSetAsync(dscFile);
        if (operationResult.IsSuccess && operationResult.Result != null)
        {
            await ConfigurationSet.UseAsync(operationResult.Result, dscFile);
        }

        SaveConfigurationCommand.NotifyCanExecuteChanged();
        IsConfigurationLoading = false;
    }

    public async Task SaveConfigurationAsAsync(string filePath)
    {
        if (IsConfigurationLoaded)
        {
            await _operationHub.ExecuteAsync(async ctx =>
            {
                try
                {
                    ctx.StartSnapshotBroadcast();
                    ctx.Start();
                    _logger.LogInformation($"Saving configuration set as {filePath}");
                    await ConfigurationSet.SaveAsAsync(filePath);
                    ctx.Success(props => props with { Message = _localizer["PreviewFile_SavedSuccessfully"] });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Saving configuration set failed");
                    ctx.Fail(props => props with { Message = _localizer["PreviewFile_SaveFailed", ex.Message] });
                }
                finally
                {
                    // After saving as a new file, re-evaluate if saving is possible
                    SaveConfigurationCommand.NotifyCanExecuteChanged();
                }
            });
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
        await _operationHub.ExecuteAsync(async ctx =>
        {
            try
            {
                ctx.StartSnapshotBroadcast();
                ctx.Start();
                _logger.LogInformation($"Creating new configuration set");
                SelectedUnit = null;
                ConfigurationSet = _setFactory();
                await AddResourceAsync();
                ctx.Complete();
            }
            catch (DSCUnitValidationException ex)
            {
                _logger.LogError(ex, "Validation of the new configuration unit failed");
                ctx.Fail(props => props with { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Creating new configuration set failed");
                ctx.Fail(props => props with { Message = _localizer["PreviewFile_CreateNewConfigurationFailed", ex.Message] });
            }
        });
    }

    [RelayCommand(CanExecute = nameof(CanSaveConfiguration))]
    private async Task OnSaveConfigurationAsync()
    {
        if (IsConfigurationLoaded)
        {
            await _operationHub.ExecuteAsync(async ctx =>
            {
                try
                {
                    ctx.StartSnapshotBroadcast();
                    ctx.Start();
                    _logger.LogInformation($"Saving configuration set");
                    await ConfigurationSet.SaveAsync();
                    ctx.Success(props => props with { Message = _localizer["PreviewFile_SaveSuccessfulMessage"] });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Saving configuration set failed");
                    ctx.Fail(props => props with { Message = _localizer["PreviewFile_SaveFailedMessage", ex.Message] });
                }
            });
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
            await _operationHub.ExecuteAsync(async ctx =>
            {
                try
                {
                    ctx.StartSnapshotBroadcast();
                    ctx.Start();
                    _logger.LogInformation($"Deleting selected unit {SelectedUnit.Item1.Title}");
                    await ConfigurationSet.RemoveAsync(SelectedUnit.Item1);
                    SelectedUnit = null;
                    ctx.Success(props => props with { Message = _localizer["PreviewFile_DeleteSuccessfulMessage"] });
                }
                catch (DSCUnitValidationException ex)
                {
                    _logger.LogError(ex, "Validation of configuration units failed after deletion");
                    ctx.Fail(props => props with { Message = ex.Message });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Deleting configuration unit failed");
                    ctx.Fail(props => props with { Message = _localizer["PreviewFile_DeleteFailedMessage", ex.Message] });
                }
            });
        }
    }

    [RelayCommand(CanExecute = nameof(CanAddUnit))]
    private async Task OnAddResourceAsync()
    {
        await _operationHub.ExecuteAsync(async ctx =>
        {
            try
            {
                ctx.StartSnapshotBroadcast();
                ctx.Start();
                _logger.LogInformation($"Adding new resource");
                await AddResourceAsync();
            }
            catch (DSCUnitValidationException ex)
            {
                _logger.LogError(ex, "Validation of the added configuration unit failed");
                ctx.Fail(props => props with { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Adding new resource failed");
                ctx.Fail(props => props with { Message = _localizer["PreviewFile_AddResourceFailedMessage", ex.Message] });
            }
        });
    }

    [RelayCommand(CanExecute = nameof(CanValidateConfiguration))]
    private async Task OnValidateConfigurationAsync()
    {
        if (IsConfigurationLoaded)
        {
            var dscFile = ConfigurationSet.GetLatestDSCFile();
            await _dscOperationHub.ExecuteValidateSetAsync(dscFile);
        }
    }

    [RelayCommand(CanExecute = nameof(CanTestConfiguration))]
    private async Task OnTestConfigurationAsync()
    {
        if (IsConfigurationLoaded)
        {
            var dscFile = ConfigurationSet.GetLatestDSCFile();
            await _dscOperationHub.ExecuteTestSetAsync(dscFile);
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
            await _operationHub.ExecuteAsync(async ctx =>
            {
                try
                {
                    ctx.StartSnapshotBroadcast();
                    ctx.Start();
                    _logger.LogInformation($"Updating selected unit {SelectedUnit.Item1.Title}");
                    await ConfigurationSet.UpdateAsync(SelectedUnit.Item1, SelectedUnit.Item2);
                    ctx.Success(props => props with { Message = _localizer["PreviewFile_ValidationFailedMessage"] });
                }
                catch (DSCUnitValidationException ex)
                {
                    _logger.LogError(ex, "Validation of configuration unit failed");
                    ctx.Fail(props => props with { Message = ex.Message });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Updating configuration unit failed");
                    ctx.Fail(props => props with { Message = _localizer["PreviewFile_UpdateFailedMessage", ex.Message] });
                }
            });
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
    }
}

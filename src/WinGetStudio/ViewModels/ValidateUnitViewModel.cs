// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Management.Configuration;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Exceptions;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Exceptions;
using WinGetStudio.Services.DesiredStateConfiguration.Extensions;
using WinGetStudio.Services.DesiredStateConfiguration.Models;
using WingetStudio.Services.VisualFeedback.Contracts;
using WingetStudio.Services.VisualFeedback.Models;

namespace WinGetStudio.ViewModels;

public delegate ValidateUnitViewModel ValidateUnitViewModelFactory();

public sealed partial class ValidateUnitViewModel : ObservableObject, IDisposable
{
    private readonly IDSC _dsc;
    private readonly IUIFeedbackService _ui;
    private readonly IStringLocalizer<ValidationViewModel> _localizer;
    private readonly ILogger<ValidationViewModel> _logger;
    private readonly IAppFrameNavigationService _navigation;
    private readonly IConfigurationManager _manager;
    private CancellationTokenSource? _cts;
    private bool _disposedValue;

    public ValidateUnitViewModel(
        IDSC dsc,
        IUIFeedbackService ui,
        IStringLocalizer<ValidationViewModel> localizer,
        ILogger<ValidationViewModel> logger,
        UnitViewModelFactory unitFactory,
        IAppFrameNavigationService navigation,
        IConfigurationManager manager)
    {
        _dsc = dsc;
        _ui = ui;
        _localizer = localizer;
        _logger = logger;
        _navigation = navigation;
        _manager = manager;
        Unit = unitFactory();
        Unit.PropertyChanged += OnUnitPropertyChanged;
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    [NotifyCanExecuteChangedFor(nameof(GetCommand))]
    [NotifyCanExecuteChangedFor(nameof(SetCommand))]
    [NotifyCanExecuteChangedFor(nameof(TestCommand))]
    private partial bool CanExecute { get; set; } = true;

    private bool CanCancel => !CanExecute;

    private SetViewModel? ActiveSet => _manager.ActiveSetPreviewState.ActiveSet;

    private ApplySetViewModel? ActiveApplySet => _manager.ActiveSetApplyState.ActiveApplySet;

    [MemberNotNullWhen(true, nameof(ActiveSet))]
    private bool IsPreviewInProgress => ActiveSet != null;

    [MemberNotNullWhen(true, nameof(ActiveApplySet))]
    private bool IsApplyInProgress => ActiveApplySet != null;

    public bool ShowNoResultState => !ShowOutputText;

    public bool ShowOutputText => !string.IsNullOrWhiteSpace(OutputText);

    [MemberNotNullWhen(true, nameof(OriginalUnit))]
    public bool CanSaveToOriginal => OriginalUnit != null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowOutputText))]
    [NotifyPropertyChangedFor(nameof(ShowNoResultState))]
    public partial string? OutputText { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSaveToOriginal))]
    [NotifyCanExecuteChangedFor(nameof(SaveToOriginalCommand))]
    public partial UnitViewModel? OriginalUnit { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Title))]
    public partial UnitViewModel Unit { get; set; }

    public string Title => string.IsNullOrWhiteSpace(Unit.Title) ? "New validation" : Unit.Title;

    [MemberNotNullWhen(true, nameof(OriginalUnit))]
    private bool CanSaveToOriginalInternal()
    {
        Debug.Assert(CanSaveToOriginal, "CanSaveToOriginalInternal called when OriginalUnit is null");

        // If there the set is being applied, we cannot update the original unit.
        if (IsApplyInProgress)
        {
            _ui.ShowTimedNotification("Cannot save to original unit while the configuration set is being applied.", NotificationMessageSeverity.Warning);
            return false;
        }

        // We can save to the original unit only if it is part of the active preview set.
        if (!IsPreviewInProgress || !ActiveSet.Units.Contains(OriginalUnit))
        {
            _ui.ShowTimedNotification("Cannot save to original unit as it is not part of the active preview configuration set.", NotificationMessageSeverity.Warning);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Retrieves the current configuration unit from the system asynchronously.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecute))]
    private async Task OnGetAsync()
    {
        await RunDscOperationAsync(async (dscUnit, cancellationToken) =>
        {
            var result = await _dsc.GetUnitAsync(dscUnit, cancellationToken);
            if (result.ResultInformation?.IsOk ?? true)
            {
                OutputText = result.Settings.ToYaml();
            }

            return result.ResultInformation;
        });
    }

    /// <summary>
    /// Sets the current machine state to the specified configuration unit asynchronously.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecute))]
    private async Task OnSetAsync()
    {
        await RunDscOperationAsync(async (dscUnit, cancellationToken) =>
        {
            var result = await _dsc.SetUnitAsync(dscUnit, cancellationToken);
            return result.ResultInformation;
        });
    }

    /// <summary>
    /// Tests whether the current machine state matches the specified configuration unit asynchronously.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecute))]
    private async Task OnTestAsync()
    {
        await RunDscOperationAsync(async (dscUnit, cancellationToken) =>
        {
            var result = await _dsc.TestUnitAsync(dscUnit, cancellationToken);
            if (result.TestResult == ConfigurationTestResult.Positive)
            {
                _ui.ShowTimedNotification(_localizer["Notification_MachineInDesiredState"], NotificationMessageSeverity.Success);
            }
            else
            {
                _ui.ShowTimedNotification(_localizer["Notification_MachineNotInDesiredState"], NotificationMessageSeverity.Error);
            }

            return result.ResultInformation;
        });
    }

    [RelayCommand(CanExecute = nameof(CanCancel))]
    private void OnCancel()
    {
        if (_cts != null && !_cts.IsCancellationRequested)
        {
            _cts.Cancel();
        }
    }

    [RelayCommand]
    private void OnBack()
    {
        _navigation.NavigateTo<ConfigurationViewModel>();
    }

    [RelayCommand(CanExecute = nameof(CanSaveToOriginal))]
    private async Task OnSaveToOriginalAsync()
    {
        try
        {
            _ui.ShowTaskProgress();
            if (IsPreviewInProgress && CanSaveToOriginalInternal())
            {
                _logger.LogInformation($"Saving changes to original unit");
                await ActiveSet.UpdateAsync(OriginalUnit, Unit);

                // If the item was selected in the preview, update it as well.
                var selectedUnit = _manager.ActiveSetPreviewState.SelectedUnit;
                if (selectedUnit?.Item1 == OriginalUnit)
                {
                    await selectedUnit.Item2.CopyFromAsync(selectedUnit.Item1);
                }

                _ui.ShowTimedNotification("Original unit updated successfully", NotificationMessageSeverity.Success);
            }
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

    /// <summary>
    /// Runs a DSC operation while managing UI feedback.
    /// </summary>
    /// <param name="action">The DSC operation to execute.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task RunDscOperationAsync(Func<IDSCUnit, CancellationToken, Task<IDSCUnitResultInformation?>> action)
    {
        try
        {
            CanExecute = false;
            _cts = new CancellationTokenSource();
            _ui.ShowTaskProgress();
            var unit = await CreateUnitAsync();
            var result = await action(unit, _cts.Token);
            if (result != null && !result.IsOk)
            {
                var title = $"0x{result.ResultCode.HResult:X}";
                List<string> messageList = [result.Description, result.Details];
                var message = string.Join(Environment.NewLine, messageList.Where(s => !string.IsNullOrEmpty(s)));
                _ui.ShowTimedNotification(title, message, NotificationMessageSeverity.Error);
            }
        }
        catch (OpenConfigurationSetException ex)
        {
            _logger.LogError(ex, "An error occurred while opening the DSC configuration set.");
            _ui.ShowTimedNotification(ex.GetErrorMessage(_localizer), NotificationMessageSeverity.Error);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogInformation(ex, "Operation canceled.");
            _ui.ShowTimedNotification("Operation canceled", NotificationMessageSeverity.Warning);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while executing a DSC operation.");
            _ui.ShowTimedNotification(ex.Message, NotificationMessageSeverity.Error);
        }
        finally
        {
            _ui.HideTaskProgress();
            _cts?.Dispose();
            _cts = null;
            CanExecute = true;
        }
    }

    /// <summary>
    /// Creates a DSC unit from the current state.
    /// </summary>
    /// <returns>The created DSC unit.</returns>
    private async Task<IDSCUnit> CreateUnitAsync()
    {
        var config = await Unit.ToConfigurationV3Async();
        var dscFile = DSCFile.CreateVirtual(config.ToYaml());
        var dscSet = await _dsc.OpenConfigurationSetAsync(dscFile);
        return dscSet.Units[0];
    }

    private void OnUnitPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(UnitViewModel.Title))
        {
            OnPropertyChanged(nameof(Title));
        }
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _cts?.Dispose();
                _cts = null;
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

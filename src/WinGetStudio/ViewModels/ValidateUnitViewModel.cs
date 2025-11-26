// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Management.Configuration;
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
    private readonly UnitViewModelFactory _unitFactory;
    private CancellationTokenSource? _cts;
    private bool _disposedValue;

    public ValidateUnitViewModel(
        IDSC dsc,
        IUIFeedbackService ui,
        IStringLocalizer<ValidationViewModel> localizer,
        ILogger<ValidationViewModel> logger,
        UnitViewModelFactory unitFactory)
    {
        _dsc = dsc;
        _ui = ui;
        _localizer = localizer;
        _logger = logger;
        _unitFactory = unitFactory;
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    [NotifyCanExecuteChangedFor(nameof(GetCommand))]
    [NotifyCanExecuteChangedFor(nameof(SetCommand))]
    [NotifyCanExecuteChangedFor(nameof(TestCommand))]
    private partial bool CanExecute { get; set; } = true;

    private bool CanCancel => !CanExecute;

    public bool ShowNoResultState => !ShowOutputText;

    public bool ShowOutputText => !string.IsNullOrWhiteSpace(OutputText);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Title))]
    public partial string? SearchResourceText { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowOutputText))]
    [NotifyPropertyChangedFor(nameof(ShowNoResultState))]
    public partial string? OutputText { get; set; }

    [ObservableProperty]
    public partial string? SettingsText { get; set; }

    public string Title => string.IsNullOrWhiteSpace(SearchResourceText) ? "New validation" : SearchResourceText;

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
        var unit = _unitFactory();
        unit.Title = SearchResourceText;
        unit.SettingsText = SettingsText;
        var config = await unit.ToConfigurationV3Async();
        var dscFile = DSCFile.CreateVirtual(config.ToYaml());
        var dscSet = await _dsc.OpenConfigurationSetAsync(dscFile);
        return dscSet.Units[0];
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

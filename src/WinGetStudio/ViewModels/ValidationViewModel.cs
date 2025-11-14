// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Management.Configuration;
using WinGetStudio.Contracts.ViewModels;
using WinGetStudio.Models;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Exceptions;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Extensions;
using WinGetStudio.Services.DesiredStateConfiguration.Models;
using WingetStudio.Services.VisualFeedback.Contracts;
using WingetStudio.Services.VisualFeedback.Models;

namespace WinGetStudio.ViewModels;

public partial class ValidationViewModel : ObservableRecipient, INavigationAware
{
    private readonly IDSC _dsc;
    private readonly IDSCProcess _dscProcess;
    private readonly IUIFeedbackService _ui;
    private readonly IStringLocalizer<ValidationViewModel> _localizer;
    private readonly ILogger<ValidationViewModel> _logger;
    private readonly UnitViewModelFactory _unitFactory;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GetCommand))]
    [NotifyCanExecuteChangedFor(nameof(SetCommand))]
    [NotifyCanExecuteChangedFor(nameof(TestCommand))]
    public partial bool CanExecuteDSCOperation { get; set; } = true;

    [ObservableProperty]
    public partial string? SearchResourceText { get; set; }

    [ObservableProperty]
    public partial string? OutputText { get; set; }

    [ObservableProperty]
    public partial string? SettingsText { get; set; }

    public ValidationViewModel(
        IDSC dsc,
        IDSCProcess dscProcess,
        IUIFeedbackService ui,
        IStringLocalizer<ValidationViewModel> localizer,
        ILogger<ValidationViewModel> logger,
        UnitViewModelFactory unitFactory)
    {
        _dsc = dsc;
        _dscProcess = dscProcess;
        _ui = ui;
        _localizer = localizer;
        _logger = logger;
        _unitFactory = unitFactory;
    }

    public void OnNavigatedTo(object parameter)
    {
        if (parameter is ValidateUnitNavigationContext context)
        {
            SearchResourceText = context.UnitToValidate.Title;
            SettingsText = context.UnitToValidate.SettingsText;
        }
    }

    public void OnNavigatedFrom()
    {
        // No-op
    }

    /// <summary>
    /// Retrieves the current configuration unit from the system asynchronously.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecuteDSCOperation))]
    private async Task OnGetAsync()
    {
        try
        {
            CanExecuteDSCOperation = false;
            _ui.ShowTaskProgress();

            _logger.LogInformation("Executing DSC get operation for resource: {Resource}", SearchResourceText);
            var result = await _dscProcess.GetResourceAsync(SearchResourceText ?? string.Empty, SettingsText ?? string.Empty);

            if (!result.IsSuccess)
            {
                _logger.LogError("DSC get failed with exit code {ExitCode}. Error: {Error}", result.ExitCode, result.Errors);
                _ui.ShowTimedNotification(
                    "DSC Get Failed",
                    $"Exit code: {result.ExitCode}\n{result.Errors}",
                    NotificationMessageSeverity.Error);
                return;
            }

            OutputText = result.Output;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while executing DSC get operation");
            _ui.ShowTimedNotification("Error", ex.Message, NotificationMessageSeverity.Error);
        }
        finally
        {
            _ui.HideTaskProgress();
            CanExecuteDSCOperation = true;
        }
    }

    /// <summary>
    /// Sets the current machine state to the specified configuration unit asynchronously.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecuteDSCOperation))]
    private async Task OnSetAsync()
    {
        try
        {
            CanExecuteDSCOperation = false;
            _ui.ShowTaskProgress();

            _logger.LogInformation("Executing DSC set operation for resource: {Resource}", SearchResourceText);
            var result = await _dscProcess.SetResourceAsync(SearchResourceText, SettingsText ?? string.Empty);

            if (!result.IsSuccess)
            {
                _logger.LogError("DSC set failed with exit code {ExitCode}. Error: {Error}", result.ExitCode, result.Errors);
                _ui.ShowTimedNotification(
                    "DSC Set Failed",
                    $"Exit code: {result.ExitCode}\n{result.Errors}",
                    NotificationMessageSeverity.Error);
                return;
            }

            OutputText = result.Output;
            _ui.ShowTimedNotification("Set Completed", "Configuration applied successfully", NotificationMessageSeverity.Success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while executing DSC set operation");
            _ui.ShowTimedNotification("Error", ex.Message, NotificationMessageSeverity.Error);
        }
        finally
        {
            _ui.HideTaskProgress();
            CanExecuteDSCOperation = true;
        }
    }

    /// <summary>
    /// Tests whether the current machine state matches the specified configuration unit asynchronously.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecuteDSCOperation))]
    private async Task OnTestAsync()
    {
        try
        {
            CanExecuteDSCOperation = false;
            _ui.ShowTaskProgress();

            _logger.LogInformation("Executing DSC test operation for resource: {Resource}", SearchResourceText);
            var result = await _dscProcess.TestResourceAsync(SearchResourceText ?? string.Empty, SettingsText ?? string.Empty);

            if (!result.IsSuccess)
            {
                _logger.LogError("DSC test failed with exit code {ExitCode}. Error: {Error}", result.ExitCode, result.Errors);
                _ui.ShowTimedNotification(
                    "DSC Test Failed",
                    $"Exit code: {result.ExitCode}\n{result.Errors}",
                    NotificationMessageSeverity.Error);
                return;
            }

            OutputText = result.Output;

            var inDesiredState = result.Output.Contains("inDesiredState: true", StringComparison.OrdinalIgnoreCase);

            if (inDesiredState)
            {
                _ui.ShowTimedNotification(_localizer["Notification_MachineInDesiredState"], NotificationMessageSeverity.Success);
            }
            else
            {
                _ui.ShowTimedNotification(_localizer["Notification_MachineNotInDesiredState"], NotificationMessageSeverity.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while executing DSC test operation");
            _ui.ShowTimedNotification("Error", ex.Message, NotificationMessageSeverity.Error);
        }
        finally
        {
            _ui.HideTaskProgress();
            CanExecuteDSCOperation = true;
        }
    }

    /// <summary>
    /// Runs a DSC operation while managing UI feedback.
    /// </summary>
    /// <param name="action">The DSC operation to execute.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task RunDscOperationAsync(Func<IDSCUnit, Task<IDSCUnitResultInformation?>> action)
    {
        try
        {
            CanExecuteDSCOperation = false;
            _ui.ShowTaskProgress();
            var unit = await CreateUnitAsync();
            var result = await action(unit);
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while executing a DSC operation.");
            _ui.ShowTimedNotification(ex.Message, NotificationMessageSeverity.Error);
        }
        finally
        {
            _ui.HideTaskProgress();
            CanExecuteDSCOperation = true;
        }
    }

    /// <summary>
    /// Creates a DSC unit from the current state.
    /// </summary>
    /// <returns>The created DSC unit.</returns>
    private async Task<IDSCUnit> CreateUnitAsync()
    {
        var unit = _unitFactory();
        unit.Title = SearchResourceText ?? string.Empty;
        unit.Settings = DSCPropertySet.FromYaml(SettingsText ?? string.Empty);
        var dscFile = DSCFile.CreateVirtual(unit.ToConfigurationV3().ToYaml());
        var dscSet = await _dsc.OpenConfigurationSetAsync(dscFile);
        return dscSet.Units[0];
    }
}

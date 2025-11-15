// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Contracts.ViewModels;
using WinGetStudio.Models;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Extensions;
using WinGetStudio.Services.DesiredStateConfiguration.Models;

namespace WinGetStudio.ViewModels;

public partial class ValidationViewModel : ObservableRecipient, INavigationAware
{
    private readonly IAppOperationHub _operationHub;
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

    public ValidationViewModel(IAppOperationHub operationHub, ILogger<ValidationViewModel> logger, UnitViewModelFactory unitFactory)
    {
        _operationHub = operationHub;
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
        await RunDscOperationAsync(async dscFile =>
        {
            var executionResult = await _operationHub.ExecuteGetUnitAsync(dscFile);
            if (executionResult.IsSuccess && executionResult.Result?.Settings != null)
            {
                OutputText = executionResult.Result.Settings.ToYaml();
            }
        });
    }

    /// <summary>
    /// Sets the current machine state to the specified configuration unit asynchronously.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecuteDSCOperation))]
    private async Task OnSetAsync()
    {
        await RunDscOperationAsync(async dscFile =>
        {
            await _operationHub.ExecuteSetUnitAsync(dscFile);
        });
    }

    /// <summary>
    /// Tests whether the current machine state matches the specified configuration unit asynchronously.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecuteDSCOperation))]
    private async Task OnTestAsync()
    {
        await RunDscOperationAsync(async dscFile =>
        {
            await _operationHub.ExecuteTestUnitAsync(dscFile);
        });
    }

    /// <summary>
    /// Runs a DSC operation with proper error handling and state management.
    /// </summary>
    /// <param name="operation">The DSC operation to execute.</param>
    private async Task RunDscOperationAsync(Func<IDSCFile, Task> operation)
    {
        try
        {
            CanExecuteDSCOperation = false;
            var dscFile = CreateDSCFile();
            await operation(dscFile);
        }
        catch (Exception ex)
        {
            // TODO notification
            _logger.LogError(ex, "An error occurred while executing the DSC operation.");
        }
        finally
        {
            CanExecuteDSCOperation = true;
        }
    }

    /// <summary>
    /// Creates a dsc file from the current input.
    /// </summary>
    /// <returns>The created dsc file.</returns>
    private IDSCFile CreateDSCFile()
    {
        var unit = _unitFactory();
        unit.Title = SearchResourceText ?? string.Empty;
        unit.Settings = DSCPropertySet.FromYaml(SettingsText ?? string.Empty);
        return DSCFile.CreateVirtual(unit.ToConfigurationV3().ToYaml());
    }
}

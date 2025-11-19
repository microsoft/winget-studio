// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Contracts.ViewModels;
using WinGetStudio.Exceptions;
using WinGetStudio.Models;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Extensions;
using WinGetStudio.Services.DesiredStateConfiguration.Models;
using WinGetStudio.Services.Operations.Extensions;

namespace WinGetStudio.ViewModels;

public partial class ValidationViewModel : ObservableRecipient, INavigationAware
{
    private readonly IAppOperationHub _operationHub;
    private readonly ILogger<ValidationViewModel> _logger;
    private readonly IStringLocalizer<ValidationViewModel> _localizer;
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
        ILogger<ValidationViewModel> logger,
        IStringLocalizer<ValidationViewModel> localizer,
        IAppOperationHub operationHub,
        UnitViewModelFactory unitFactory)
    {
        _logger = logger;
        _localizer = localizer;
        _operationHub = operationHub;
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
        CanExecuteDSCOperation = false;
        await _operationHub.RunWithProgressAsync(
            props => props with
            {
                Title = SearchResourceText,
                Message = _localizer["GetUnitOperation_PreStartMessage"],
            },
            async (context, factory) =>
            {
                try
                {
                    var dscFile = CreateDSCFile();
                    var getUnit = factory.CreateGetUnitOperation(dscFile);
                    var operationResult = await getUnit.ExecuteAsync(context);
                    if (operationResult.IsSuccess && operationResult.Result?.Settings != null)
                    {
                        OutputText = operationResult.Result.Settings.ToYaml();
                    }
                }
                catch (DSCUnitValidationException ex)
                {
                    _logger.LogError(ex, "A validation error occurred while getting the unit settings.");
                    context.Fail(props => props with { Message = ex.Message });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while getting the unit settings.");
                    context.Fail(props => props with { Message = _localizer["GetUnitOperation_UnexpectedErrorMessage", ex.Message] });
                }
            });
        CanExecuteDSCOperation = true;
    }

    /// <summary>
    /// Sets the current machine state to the specified configuration unit asynchronously.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecuteDSCOperation))]
    private async Task OnSetAsync()
    {
        CanExecuteDSCOperation = false;
        await _operationHub.RunWithProgressAsync(
            props => props with
            {
                Title = SearchResourceText,
                Message = _localizer["SetUnitOperation_PreStartMessage"],
            },
            async (context, factory) =>
            {
                try
                {
                    var dscFile = CreateDSCFile();
                    var setUnit = factory.CreateSetUnitOperation(dscFile);
                    await setUnit.ExecuteAsync(context);
                }
                catch (DSCUnitValidationException ex)
                {
                    _logger.LogError(ex, "A validation error occurred while applying the unit settings.");
                    context.Fail(props => props with { Message = ex.Message });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while applying the unit settings.");
                    context.Fail(props => props with { Message = _localizer["SetUnitOperation_UnexpectedErrorMessage", ex.Message] });
                }
            });
        CanExecuteDSCOperation = true;
    }

    /// <summary>
    /// Tests whether the current machine state matches the specified configuration unit asynchronously.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecuteDSCOperation))]
    private async Task OnTestAsync()
    {
        CanExecuteDSCOperation = false;
        await _operationHub.RunWithProgressAsync(
            props => props with
            {
                Title = SearchResourceText,
                Message = _localizer["TestUnitOperation_PreStartMessage"],
            },
            async (context, factory) =>
            {
                try
                {
                    var dscFile = CreateDSCFile();
                    var testUnit = factory.CreateTestUnitOperation(dscFile);
                    await testUnit.ExecuteAsync(context);
                }
                catch (DSCUnitValidationException ex)
                {
                    _logger.LogError(ex, "A validation error occurred while testing the unit settings.");
                    context.Fail(props => props with { Message = ex.Message });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while testing the unit settings.");
                    context.Fail(props => props with { Message = _localizer["TestUnitOperation_UnexpectedErrorMessage", ex.Message] });
                }
            });
        CanExecuteDSCOperation = true;
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

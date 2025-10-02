// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Management.Configuration;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Exceptions;
using WinGetStudio.ViewModels;

namespace WinGetStudio.Models;

public partial class ApplySetUnit : ObservableObject
{
    private readonly IStringLocalizer _localizer;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLoading))]
    [NotifyPropertyChangedFor(nameof(IsExpanded))]
    public partial ApplySetUnitState State { get; set; }

    [ObservableProperty]
    public partial string? Message { get; set; }

    [ObservableProperty]
    public partial string? Description { get; set; }

    public bool IsLoading => State == ApplySetUnitState.InProgress;

    public bool IsExpanded => State == ApplySetUnitState.Failed || State == ApplySetUnitState.Skipped;

    public DSCConfigurationUnitViewModel Unit { get; }

    public ApplySetUnit(IDSCUnit unit, IStringLocalizer localizer, ILogger logger)
    {
        Unit = new(unit, logger);
        _localizer = localizer;
        Update(ApplySetUnitState.NotStarted);
    }

    public void Update(ApplySetUnitState state, IDSCUnitResultInformation? resultInformation = null)
    {
        State = state;
        if (State == ApplySetUnitState.Succeeded)
        {
            Message = _localizer["ConfigurationUnitSuccess"];
        }
        else if (State == ApplySetUnitState.NotStarted)
        {
            Message = _localizer["ConfigurationUnitNotStarted"];
        }
        else if (State == ApplySetUnitState.Failed && resultInformation != null)
        {
            Message = GetUnitErrorMessage(resultInformation);
            Description = GetErrorDescription(resultInformation);
        }
        else if (State == ApplySetUnitState.Skipped && resultInformation != null)
        {
            Message = GetUnitSkipMessage(resultInformation);
            Description = GetErrorDescription(resultInformation);
        }
    }

    private string GetUnitSkipMessage(IDSCUnitResultInformation resultInformation)
    {
        var hresult = resultInformation.ResultCode.HResult;
        switch (hresult)
        {
            case ConfigurationException.WingetConfigErrorManuallySkipped:
                return _localizer["ConfigurationUnitManuallySkipped"];
            case ConfigurationException.WingetConfigErrorDependencyUnsatisfied:
                return _localizer["ConfigurationUnitNotRunDueToDependency"];
            case ConfigurationException.WingetConfigErrorAssertionFailed:
                return _localizer["ConfigurationUnitNotRunDueToFailedAssert"];
        }

        var resultCodeHex = $"0x{hresult:X}";
        return _localizer["ConfigurationUnitSkipped", resultCodeHex];
    }

    private string GetUnitErrorMessage(IDSCUnitResultInformation resultInformation)
    {
        var hresult = resultInformation.ResultCode.HResult;
        switch (hresult)
        {
            case ConfigurationException.WingetConfigErrorDuplicateIdentifier:
                return _localizer["ConfigurationUnitHasDuplicateIdentifier", Unit.Id];
            case ConfigurationException.WingetConfigErrorMissingDependency:
                return _localizer["ConfigurationUnitHasMissingDependency", resultInformation.Details];
            case ConfigurationException.WingetConfigErrorAssertionFailed:
                return _localizer["ConfigurationUnitAssertHadNegativeResult"];
            case ConfigurationException.WinGetConfigUnitNotFound:
                return _localizer["ConfigurationUnitNotFoundInModule"];
            case ConfigurationException.WinGetConfigUnitNotFoundRepository:
                return _localizer["ConfigurationUnitNotFound"];
            case ConfigurationException.WinGetConfigUnitMultipleMatches:
                return _localizer["ConfigurationUnitMultipleMatches"];
            case ConfigurationException.WinGetConfigUnitInvokeGet:
                return _localizer["ConfigurationUnitFailedDuringGet"];
            case ConfigurationException.WinGetConfigUnitInvokeTest:
                return _localizer["ConfigurationUnitFailedDuringTest"];
            case ConfigurationException.WinGetConfigUnitInvokeSet:
                return _localizer["ConfigurationUnitFailedDuringSet"];
            case ConfigurationException.WinGetConfigUnitModuleConflict:
                return _localizer["ConfigurationUnitModuleConflict"];
            case ConfigurationException.WinGetConfigUnitImportModule:
                return _localizer["ConfigurationUnitModuleImportFailed"];
            case ConfigurationException.WinGetConfigUnitInvokeInvalidResult:
                return _localizer["ConfigurationUnitReturnedInvalidResult"];
            case ConfigurationException.WingetConfigErrorManuallySkipped:
                return _localizer["ConfigurationUnitManuallySkipped"];
            case ConfigurationException.WingetConfigErrorDependencyUnsatisfied:
                return _localizer["ConfigurationUnitNotRunDueToDependency"];
            case ConfigurationException.WinGetConfigUnitSettingConfigRoot:
                return _localizer["WinGetConfigUnitSettingConfigRoot"];
            case ConfigurationException.WinGetConfigUnitImportModuleAdmin:
                return _localizer["WinGetConfigUnitImportModuleAdmin"];
        }

        var resultCodeHex = $"0x{hresult:X}";
        switch (resultInformation.ResultSource)
        {
            case ConfigurationUnitResultSource.ConfigurationSet:
                return _localizer["ConfigurationUnitFailedConfigSet", resultCodeHex];
            case ConfigurationUnitResultSource.Internal:
                return _localizer["ConfigurationUnitFailedInternal", resultCodeHex];
            case ConfigurationUnitResultSource.Precondition:
                return _localizer["ConfigurationUnitFailedPrecondition", resultCodeHex];
            case ConfigurationUnitResultSource.SystemState:
                return _localizer["ConfigurationUnitFailedSystemState", resultCodeHex];
            case ConfigurationUnitResultSource.UnitProcessing:
                return _localizer["ConfigurationUnitFailedUnitProcessing", resultCodeHex];
        }

        return _localizer["ConfigurationUnitFailed", resultCodeHex];
    }

    private string GetErrorDescription(IDSCUnitResultInformation resultInformation)
    {
        if (string.IsNullOrEmpty(resultInformation.Description))
        {
            return string.Empty;
        }

        // If the localized configuration error message requires additional
        // context, display the error description from the resource module directly.
        // Code reference: https://github.com/microsoft/winget-cli/blob/master/src/AppInstallerCLICore/Workflows/ConfigurationFlow.cpp
        switch (resultInformation.ResultCode.HResult)
        {
            case ConfigurationException.WingetConfigErrorDuplicateIdentifier:
            case ConfigurationException.WingetConfigErrorMissingDependency:
            case ConfigurationException.WingetConfigErrorAssertionFailed:
            case ConfigurationException.WinGetConfigUnitNotFound:
            case ConfigurationException.WinGetConfigUnitNotFoundRepository:
            case ConfigurationException.WinGetConfigUnitMultipleMatches:
            case ConfigurationException.WinGetConfigUnitModuleConflict:
            case ConfigurationException.WinGetConfigUnitImportModule:
            case ConfigurationException.WinGetConfigUnitInvokeInvalidResult:
            case ConfigurationException.WinGetConfigUnitSettingConfigRoot:
            case ConfigurationException.WinGetConfigUnitImportModuleAdmin:
                return string.Empty;
            default:
                return resultInformation.Description;
        }
    }
}

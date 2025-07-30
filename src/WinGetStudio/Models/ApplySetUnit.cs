// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using WinGetStudio.Contracts.Services;
using WinGetStudio.ViewModels;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Exceptions;
using Microsoft.Management.Configuration;

namespace WinGetStudio.Models;

public partial class ApplySetUnit : ObservableObject
{
    private readonly IStringResource _stringResource;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLoading))]
    [NotifyPropertyChangedFor(nameof(IsExpanded))]
    private ApplySetUnitState _state;

    [ObservableProperty]
    private string? _message;

    [ObservableProperty]
    private string? _description;

    public bool IsLoading => State == ApplySetUnitState.InProgress;

    public bool IsExpanded => State == ApplySetUnitState.Failed || State == ApplySetUnitState.Skipped;

    public DSCConfigurationUnitViewModel Unit { get; }

    public ApplySetUnit(IDSCUnit unit, IStringResource stringResource)
    {
        Unit = new(unit);
        _stringResource = stringResource;
        Update(ApplySetUnitState.NotStarted);
    }

    public void Update(ApplySetUnitState state, IDSCUnitResultInformation? resultInformation = null)
    {
        State = state;
        if (State == ApplySetUnitState.Succeeded)
        {
            Message = _stringResource.GetLocalized("ConfigurationUnitSuccess");
        }
        else if(State == ApplySetUnitState.NotStarted)
        {
            Message = _stringResource.GetLocalized("ConfigurationUnitNotStarted");
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
                return _stringResource.GetLocalized("ConfigurationUnitManuallySkipped");
            case ConfigurationException.WingetConfigErrorDependencyUnsatisfied:
                return _stringResource.GetLocalized("ConfigurationUnitNotRunDueToDependency");
            case ConfigurationException.WingetConfigErrorAssertionFailed:
                return _stringResource.GetLocalized("ConfigurationUnitNotRunDueToFailedAssert");
        }

        var resultCodeHex = $"0x{hresult:X}";
        return _stringResource.GetLocalized("ConfigurationUnitSkipped", resultCodeHex);
    }

    private string GetUnitErrorMessage(IDSCUnitResultInformation resultInformation)
    {
        var hresult = resultInformation.ResultCode.HResult;
        switch (hresult)
        {
            case ConfigurationException.WingetConfigErrorDuplicateIdentifier:
                return _stringResource.GetLocalized("ConfigurationUnitHasDuplicateIdentifier", Unit.Id);
            case ConfigurationException.WingetConfigErrorMissingDependency:
                return _stringResource.GetLocalized("ConfigurationUnitHasMissingDependency", resultInformation.Details);
            case ConfigurationException.WingetConfigErrorAssertionFailed:
                return _stringResource.GetLocalized("ConfigurationUnitAssertHadNegativeResult");
            case ConfigurationException.WinGetConfigUnitNotFound:
                return _stringResource.GetLocalized("ConfigurationUnitNotFoundInModule");
            case ConfigurationException.WinGetConfigUnitNotFoundRepository:
                return _stringResource.GetLocalized("ConfigurationUnitNotFound");
            case ConfigurationException.WinGetConfigUnitMultipleMatches:
                return _stringResource.GetLocalized("ConfigurationUnitMultipleMatches");
            case ConfigurationException.WinGetConfigUnitInvokeGet:
                return _stringResource.GetLocalized("ConfigurationUnitFailedDuringGet");
            case ConfigurationException.WinGetConfigUnitInvokeTest:
                return _stringResource.GetLocalized("ConfigurationUnitFailedDuringTest");
            case ConfigurationException.WinGetConfigUnitInvokeSet:
                return _stringResource.GetLocalized("ConfigurationUnitFailedDuringSet");
            case ConfigurationException.WinGetConfigUnitModuleConflict:
                return _stringResource.GetLocalized("ConfigurationUnitModuleConflict");
            case ConfigurationException.WinGetConfigUnitImportModule:
                return _stringResource.GetLocalized("ConfigurationUnitModuleImportFailed");
            case ConfigurationException.WinGetConfigUnitInvokeInvalidResult:
                return _stringResource.GetLocalized("ConfigurationUnitReturnedInvalidResult");
            case ConfigurationException.WingetConfigErrorManuallySkipped:
                return _stringResource.GetLocalized("ConfigurationUnitManuallySkipped");
            case ConfigurationException.WingetConfigErrorDependencyUnsatisfied:
                return _stringResource.GetLocalized("ConfigurationUnitNotRunDueToDependency");
            case ConfigurationException.WinGetConfigUnitSettingConfigRoot:
                return _stringResource.GetLocalized("WinGetConfigUnitSettingConfigRoot");
            case ConfigurationException.WinGetConfigUnitImportModuleAdmin:
                return _stringResource.GetLocalized("WinGetConfigUnitImportModuleAdmin");
        }

        var resultCodeHex = $"0x{hresult:X}";
        switch (resultInformation.ResultSource)
        {
            case ConfigurationUnitResultSource.ConfigurationSet:
                return _stringResource.GetLocalized("ConfigurationUnitFailedConfigSet", resultCodeHex);
            case ConfigurationUnitResultSource.Internal:
                return _stringResource.GetLocalized("ConfigurationUnitFailedInternal", resultCodeHex);
            case ConfigurationUnitResultSource.Precondition:
                return _stringResource.GetLocalized("ConfigurationUnitFailedPrecondition", resultCodeHex);
            case ConfigurationUnitResultSource.SystemState:
                return _stringResource.GetLocalized("ConfigurationUnitFailedSystemState", resultCodeHex);
            case ConfigurationUnitResultSource.UnitProcessing:
                return _stringResource.GetLocalized("ConfigurationUnitFailedUnitProcessing", resultCodeHex);
        }

        return _stringResource.GetLocalized("ConfigurationUnitFailed", resultCodeHex);
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

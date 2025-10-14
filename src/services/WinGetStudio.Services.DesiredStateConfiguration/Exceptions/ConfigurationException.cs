// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Extensions.Localization;

namespace WinGetStudio.Services.DesiredStateConfiguration.Exceptions;

public class ConfigurationException : Exception
{
    // WinGet Configuration error codes:
    // https://github.com/microsoft/winget-cli/blob/master/src/PowerShell/Microsoft.WinGet.Configuration.Engine/Exceptions/ErrorCodes.cs
    public const int WingetConfigErrorInvalidConfigurationFile = unchecked((int)0x8A15C001);
    public const int WingetConfigErrorInvalidYaml = unchecked((int)0x8A15C002);
    public const int WingetConfigErrorInvalidFieldType = unchecked((int)0x8A15C003);
    public const int WingetConfigErrorUnknownConfigurationFileVersion = unchecked((int)0x8A15C004);
    public const int WingetConfigErrorSetApplyFailed = unchecked((int)0x8A15C005);
    public const int WingetConfigErrorDuplicateIdentifier = unchecked((int)0x8A15C006);
    public const int WingetConfigErrorMissingDependency = unchecked((int)0x8A15C007);
    public const int WingetConfigErrorDependencyUnsatisfied = unchecked((int)0x8A15C008);
    public const int WingetConfigErrorAssertionFailed = unchecked((int)0x8A15C009);
    public const int WingetConfigErrorManuallySkipped = unchecked((int)0x8A15C00A);
    public const int WingetConfigErrorWarningNotAccepted = unchecked((int)0x8A15C00B);
    public const int WingetConfigErrorSetDependencyCycle = unchecked((int)0x8A15C00C);
    public const int WingetConfigErrorInvalidFieldValue = unchecked((int)0x8A15C00D);
    public const int WingetConfigErrorMissingField = unchecked((int)0x8A15C00E);
    public const int WinGetConfigErrorTestFailed = unchecked((int)0x8A15C00F);
    public const int WinGetConfigErrorTestNotRun = unchecked((int)0x8A15C010);
    public const int WinGetConfigErrorGetFailed = unchecked((int)0x8A15C011);

    // WinGet Configuration unit error codes:
    public const int WinGetConfigUnitNotFound = unchecked((int)0x8A15C101);
    public const int WinGetConfigUnitNotFoundRepository = unchecked((int)0x8A15C102);
    public const int WinGetConfigUnitMultipleMatches = unchecked((int)0x8A15C103);
    public const int WinGetConfigUnitInvokeGet = unchecked((int)0x8A15C104);
    public const int WinGetConfigUnitInvokeTest = unchecked((int)0x8A15C105);
    public const int WinGetConfigUnitInvokeSet = unchecked((int)0x8A15C106);
    public const int WinGetConfigUnitModuleConflict = unchecked((int)0x8A15C107);
    public const int WinGetConfigUnitImportModule = unchecked((int)0x8A15C108);
    public const int WinGetConfigUnitInvokeInvalidResult = unchecked((int)0x8A15C109);
    public const int WinGetConfigUnitSettingConfigRoot = unchecked((int)0x8A15C110);
    public const int WinGetConfigUnitImportModuleAdmin = unchecked((int)0x8A15C111);

    public static bool TryGetSkipMessage(IStringLocalizer localizer, int errorCode, out string message)
    {
        // Override for the dependency unsatisfied case to provide a more specific message
        if (errorCode == WingetConfigErrorDependencyUnsatisfied)
        {
            message = localizer["ConfigurationUnitNotRunDueToDependency"];
            return true;
        }

        return TryGetErrorMessage(localizer, errorCode, out message);
    }

    public static bool TryGetErrorMessage(IStringLocalizer localizer, int errorCode, out string message)
    {
        message = null;
        switch (errorCode)
        {
            case WingetConfigErrorInvalidConfigurationFile:
                message = localizer["ConfigurationFileInvalid"];
                return true;
            case WingetConfigErrorInvalidYaml:
                message = localizer["ConfigurationYamlInvalid"];
                return true;
            case WingetConfigErrorInvalidFieldType:
                // expects a field name as argument
                return false;
            case WingetConfigErrorUnknownConfigurationFileVersion:
                // expects a version number as argument
                return false;
            case WingetConfigErrorSetApplyFailed:
                message = localizer["ConfigurationSetApplyFailed"];
                return true;
            case WingetConfigErrorDuplicateIdentifier:
                // expects an identifier as argument
                return false;
            case WingetConfigErrorMissingDependency:
                // expects details as argument
                return false;
            case WingetConfigErrorDependencyUnsatisfied:
                message = localizer["ConfigurationUnitIsPartOfDependencyCycle"];
                return true;
            case WingetConfigErrorAssertionFailed:
                message = localizer["ConfigurationUnitAssertHadNegativeResult"];
                return true;
            case WingetConfigErrorManuallySkipped:
                message = localizer["ConfigurationUnitManuallySkipped"];
                return true;
            case WingetConfigErrorWarningNotAccepted:
                message = localizer["ConfigurationWarningNotAccepted"];
                return true;
            case WingetConfigErrorSetDependencyCycle:
                message = localizer["ConfigurationSetDependencyCycle"];
                return true;
            case WingetConfigErrorInvalidFieldValue:
                // expects a field name as argument
                return false;
            case WingetConfigErrorMissingField:
                // expects a field name as argument
                return false;
            case WinGetConfigErrorTestFailed:
                message = localizer["ConfigurationTestFailed"];
                return true;
            case WinGetConfigErrorTestNotRun:
                message = localizer["ConfigurationTestNotRun"];
                return true;
            case WinGetConfigErrorGetFailed:
                message = localizer["ConfigurationGetFailed"];
                return true;
            case WinGetConfigUnitNotFound:
                message = localizer["ConfigurationUnitNotFoundInModule"];
                return true;
            case WinGetConfigUnitNotFoundRepository:
                message = localizer["ConfigurationUnitNotFound"];
                return true;
            case WinGetConfigUnitMultipleMatches:
                message = localizer["ConfigurationUnitNotFound"];
                return true;
            case WinGetConfigUnitInvokeGet:
                message = localizer["ConfigurationUnitFailedDuringGet"];
                return true;
            case WinGetConfigUnitInvokeTest:
                message = localizer["ConfigurationUnitFailedDuringTest"];
                return true;
            case WinGetConfigUnitInvokeSet:
                message = localizer["ConfigurationUnitFailedDuringSet"];
                return true;
            case WinGetConfigUnitModuleConflict:
                message = localizer["ConfigurationUnitModuleConflict"];
                return true;
            case WinGetConfigUnitImportModule:
                message = localizer["ConfigurationUnitModuleImportFailed"];
                return true;
            case WinGetConfigUnitInvokeInvalidResult:
                message = localizer["ConfigurationUnitReturnedInvalidResult"];
                return true;
            case WinGetConfigUnitSettingConfigRoot:
                message = localizer["ConfigurationUnitSettingConfigRoot"];
                return true;
            case WinGetConfigUnitImportModuleAdmin:
                message = localizer["ConfigurationUnitImportModuleAdmin"];
                return true;
            default:
                return false;
        }
    }
}

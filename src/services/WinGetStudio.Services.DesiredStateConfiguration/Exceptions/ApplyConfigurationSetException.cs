// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using Microsoft.Extensions.Localization;
using Microsoft.Management.Configuration;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;

namespace WinGetStudio.Services.DesiredStateConfiguration.Exceptions;

public sealed partial class ApplyConfigurationSetException : ConfigurationException
{
    public IDSCApplySetResult ApplySetResult { get; }

    public ApplyConfigurationSetException(IDSCApplySetResult result)
    {
        ApplySetResult = result;
    }

    /// <summary>
    /// Gets the error message for the apply set result, providing more specific
    /// messages for known error codes.
    /// </summary>
    /// <param name="localizer">The localizer to use for retrieving localized strings.</param>
    /// <returns>The error message.</returns>
    public string GetSetErrorMessage(IStringLocalizer localizer)
    {
        Debug.Assert(ApplySetResult?.ResultCode != null, "Result code should not be null for a failed apply operation.");
        var resultCode = ApplySetResult.ResultCode;
        var hresult = resultCode.HResult;

        // Try to get a known error message for the HRESULT
        if (TryGetErrorMessage(localizer, hresult, out var errorMessage))
        {
            return errorMessage;
        }

        // Fallback to a generic error message
        return localizer["ConfigurationSetApplyFailed"];
    }

    /// <summary>
    /// Gets the skip message for a unit result, providing more specific
    /// messages for known skip codes.
    /// </summary>
    /// <param name="localizer">The localizer to use for retrieving localized strings.</param>
    /// <param name="resultInformation">The unit result information to get the skip message for.</param>
    /// <returns>The skip message.</returns>
    public static string GetUnitSkipMessage(IStringLocalizer localizer, IDSCUnitResultInformation resultInformation)
    {
        var hresult = resultInformation.ResultCode.HResult;

        // Try to get a known skip message for the HRESULT
        if (TryGetSkipMessage(localizer, hresult, out var skipMessage))
        {
            return skipMessage;
        }

        // Fallback to a generic skip message with the HRESULT code
        var resultCodeHex = $"0x{hresult:X}";
        return localizer["ConfigurationUnitSkipped", resultCodeHex];
    }

    /// <summary>
    /// Gets the error message for a unit result, providing more specific
    /// messages for known error codes.
    /// </summary>
    /// <param name="localizer">The localizer to use for retrieving localized strings.</param>
    /// <param name="resultInformation">The unit result information to get the error message for.</param>
    /// <returns>The error message.</returns>
    public static string GetUnitErrorMessage(IStringLocalizer localizer, IDSCUnit unit, IDSCUnitResultInformation resultInformation)
    {
        var hresult = resultInformation.ResultCode.HResult;

        // Provide more specific messages for certain known error codes
        switch (hresult)
        {
            case WingetConfigErrorDuplicateIdentifier:
                return localizer["ConfigurationUnitHasDuplicateIdentifier", unit.Id];
            case WingetConfigErrorMissingDependency:
                return localizer["ConfigurationUnitHasMissingDependency", resultInformation.Details];
        }

        // Try to get a known error message for the HRESULT
        if (TryGetErrorMessage(localizer, hresult, out var errorMessage))
        {
            return errorMessage;
        }

        // Fallback to a generic error message with the HRESULT code and source
        var resultCodeHex = $"0x{hresult:X}";
        return resultInformation.ResultSource switch
        {
            ConfigurationUnitResultSource.ConfigurationSet => localizer["ConfigurationUnitFailedConfigSet", resultCodeHex],
            ConfigurationUnitResultSource.Internal => localizer["ConfigurationUnitFailedInternal", resultCodeHex],
            ConfigurationUnitResultSource.Precondition => localizer["ConfigurationUnitFailedPrecondition", resultCodeHex],
            ConfigurationUnitResultSource.SystemState => localizer["ConfigurationUnitFailedSystemState", resultCodeHex],
            ConfigurationUnitResultSource.UnitProcessing => localizer["ConfigurationUnitFailedUnitProcessing", resultCodeHex],
            _ => localizer["ConfigurationUnitFailed", resultCodeHex],
        };
    }

    /// <summary>
    /// Gets the error description from the unit result, if available.
    /// </summary>
    /// <param name="resultInformation">The unit result information to get the error description from.</param>
    /// <returns>The error description, or an empty string if none is available or applicable.</returns>
    public static string GetErrorDescription(IDSCUnitResultInformation resultInformation)
    {
        if (string.IsNullOrEmpty(resultInformation?.Description))
        {
            return string.Empty;
        }

        // If the localized configuration error message requires additional
        // context, display the error description from the resource module directly.
        // Code reference: https://github.com/microsoft/winget-cli/blob/master/src/AppInstallerCLICore/Workflows/ConfigurationFlow.cpp
        switch (resultInformation.ResultCode.HResult)
        {
            case WingetConfigErrorDuplicateIdentifier:
            case WingetConfigErrorMissingDependency:
            case WingetConfigErrorAssertionFailed:
            case WinGetConfigUnitNotFound:
            case WinGetConfigUnitNotFoundRepository:
            case WinGetConfigUnitMultipleMatches:
            case WinGetConfigUnitModuleConflict:
            case WinGetConfigUnitImportModule:
            case WinGetConfigUnitInvokeInvalidResult:
            case WinGetConfigUnitSettingConfigRoot:
            case WinGetConfigUnitImportModuleAdmin:
                return string.Empty;
            default:
                return resultInformation.Description;
        }
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Extensions.Localization;
using Microsoft.Management.Configuration;

namespace WinGetStudio.Services.DesiredStateConfiguration.Exceptions;

public class OpenConfigurationSetException : ConfigurationException
{
    /// <summary>
    /// Gets the <see cref="OpenConfigurationSetResult.ResultCode"/>
    /// </summary>
    public Exception ResultCode { get; }

    /// <summary>
    /// Gets the field that is missing/invalid, if appropriate for the specific ResultCode.
    /// </summary>
    public string Field { get; }

    /// <summary>
    /// Gets the value of the field, if appropriate for the specific ResultCode.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Gets the line number for the failure reason, if determined.
    /// </summary>
    public uint Line { get; }

    /// <summary>
    /// Gets the column number for the failure reason, if determined.
    /// </summary>
    public uint Column { get; }

    public OpenConfigurationSetException(OpenConfigurationSetResult result)
    {
        ResultCode = result.ResultCode;
        Field = result.Field;
        Value = result.Value;
        Line = result.Line;
        Column = result.Column;
    }

    /// <summary>
    /// Gets the localized error message for this exception.
    /// </summary>
    /// <param name="localizer">The localizer to use.</param>
    /// <returns>The localized error message.</returns>
    public string GetErrorMessage(IStringLocalizer localizer)
    {
        var hresult = ResultCode.HResult;
        var message = hresult switch
        {
            WingetConfigErrorInvalidFieldType => localizer["ConfigurationFieldInvalidType", Field],
            WingetConfigErrorInvalidFieldValue => localizer["ConfigurationFieldInvalidValue", Field, Value],
            WingetConfigErrorMissingField => localizer["ConfigurationFieldMissing", Field],
            WingetConfigErrorUnknownConfigurationFileVersion => localizer["ConfigurationFileVersionUnknown", Value],
            _ => TryGetErrorMessage(localizer, hresult, out var errorMessage) ? errorMessage : localizer["ConfigurationFileInvalid"],
        };

        var position = string.Empty;
        if (Line > 0)
        {
            position = $" {localizer["ConfigurationErrorLineAndRow", Line, Column]}";
        }

        return $"{message}{position}";
    }
}

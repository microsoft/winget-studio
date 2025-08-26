// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Management.Configuration;

namespace WinGetStudio.Services.DesiredStateConfiguration.Contracts;

public interface IDSCUnitResultInformation
{
    /// <summary>
    /// Gets the error code of the result.
    /// </summary>
    Exception ResultCode { get; }

    /// <summary>
    /// Gets the short description of the failure.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets a more detailed error message appropriate for diagnosing the root cause of an error.
    /// </summary>
    string Details { get; }

    /// <summary>
    /// Gets the source of the result.
    /// </summary>
    ConfigurationUnitResultSource ResultSource { get; }

    /// <summary>
    /// Gets a value indicating whether the result indicates success or failure.
    /// </summary>
    bool IsOk => ResultCode == null;
}

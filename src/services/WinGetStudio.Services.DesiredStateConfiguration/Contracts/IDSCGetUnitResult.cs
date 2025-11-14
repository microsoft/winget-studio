// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.Services.DesiredStateConfiguration.Models;

namespace WinGetStudio.Services.DesiredStateConfiguration.Contracts;

public interface IDSCGetUnitResult
{
    /// <summary>
    /// Gets the result information for the operation.
    /// </summary>
    IDSCUnitResultInformation ResultInformation { get; }

    /// <summary>
    /// Gets the settings for the configuration unit.
    /// </summary>
    DSCPropertySet Settings { get; }
}

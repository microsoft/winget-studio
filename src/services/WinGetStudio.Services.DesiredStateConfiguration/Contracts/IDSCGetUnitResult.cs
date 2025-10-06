// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

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
    IReadOnlyDictionary<string, object> Settings { get; }
}

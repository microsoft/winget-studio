// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WinGetStudio.Services.DesiredStateConfiguration.Contracts;

public interface IDSCGetUnitDetailsResult
{
    /// <summary>
    /// Gets the configuration unit whose details were retrieved.
    /// </summary>
    IDSCUnit Unit { get; }

    /// <summary>
    /// Gets the details, if they were able to be acquired successfully.
    /// </summary>
    IDSCUnitDetails UnitDetails { get; }

    /// <summary>
    /// Gets the result of getting the configuration unit details.
    /// </summary>
    IDSCUnitResultInformation ResultInformation { get; }
}

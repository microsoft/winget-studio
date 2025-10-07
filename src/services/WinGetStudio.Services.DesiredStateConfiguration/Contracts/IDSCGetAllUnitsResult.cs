// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace WinGetStudio.Services.DesiredStateConfiguration.Contracts;

public interface IDSCGetAllUnitsResult
{
    /// <summary>
    /// Gets the list of configuration units.
    /// </summary>
    IReadOnlyList<IDSCUnit> Units { get; }

    /// <summary>
    /// Gets result information about the operation.
    /// </summary>
    IDSCUnitResultInformation ResultInformation { get; }
}

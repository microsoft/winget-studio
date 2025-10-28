// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace WinGetStudio.Services.DesiredStateConfiguration.Contracts;

public interface IDSCGetSetDetailsResult
{
    /// <summary>
    /// Gets the list of unit results.
    /// </summary>
    IReadOnlyList<IDSCGetUnitDetailsResult> UnitResults { get; }
}

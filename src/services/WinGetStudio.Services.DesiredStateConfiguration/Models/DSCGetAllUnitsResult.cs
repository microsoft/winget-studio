// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Management.Configuration;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;

namespace WinGetStudio.Services.DesiredStateConfiguration.Models;

internal sealed class DSCGetAllUnitsResult : IDSCGetAllUnitsResult
{
    /// <inheritdoc/>
    public IReadOnlyList<IDSCUnit> Units { get; }

    /// <inheritdoc/>
    public IDSCUnitResultInformation ResultInformation { get; }

    public DSCGetAllUnitsResult(GetAllConfigurationUnitsResult unitsResult)
    {
        Units = [..unitsResult.Units.Select(unit => new DSCUnit(unit))];
        ResultInformation = new DSCUnitResultInformation(unitsResult.ResultInformation);
    }
}

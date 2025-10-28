// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Management.Configuration;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;

namespace WinGetStudio.Services.DesiredStateConfiguration.Models;

internal sealed partial class DSCGetSetDetailsResult : IDSCGetSetDetailsResult
{
    /// <inheritdoc/>
    public IReadOnlyList<IDSCGetUnitDetailsResult> UnitResults { get; }

    public DSCGetSetDetailsResult(GetConfigurationSetDetailsResult result)
    {
        // Constructor copies all the required data from the out-of-proc COM
        // objects over to the current process. This ensures that we have this
        // information available even if the out-of-proc COM objects are no
        // longer available (e.g. AppInstaller service is no longer running).
        var unitResults = new List<IDSCGetUnitDetailsResult>();
        foreach (var unitResult in result.UnitResults)
        {
            unitResults.Add(new DSCGetUnitDetailsResult(unitResult));
        }

        UnitResults = unitResults;
    }
}

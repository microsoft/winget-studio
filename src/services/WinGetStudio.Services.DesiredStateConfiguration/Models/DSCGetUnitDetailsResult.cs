// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Management.Configuration;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;

namespace WinGetStudio.Services.DesiredStateConfiguration.Models;

internal sealed partial class DSCGetUnitDetailsResult : IDSCGetUnitDetailsResult
{
    /// <inheritdoc/>
    public IDSCUnit Unit { get; }

    /// <inheritdoc/>
    public IDSCUnitDetails UnitDetails { get; }

    /// <inheritdoc/>
    public IDSCUnitResultInformation ResultInformation { get; }

    public DSCGetUnitDetailsResult(GetConfigurationUnitDetailsResult result)
    {
        // Constructor copies all the required data from the out-of-proc COM
        // objects over to the current process. This ensures that we have this
        // information available even if the out-of-proc COM objects are no
        // longer available (e.g. AppInstaller service is no longer running).
        Unit = new DSCUnit(result.Unit);
        UnitDetails = result.Details == null ? null : new DSCUnitDetails(result.Details);
        ResultInformation = result.ResultInformation == null ? null : new DSCUnitResultInformation(result.ResultInformation);
    }
}

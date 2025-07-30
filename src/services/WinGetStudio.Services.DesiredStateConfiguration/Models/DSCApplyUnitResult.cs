// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using Microsoft.Management.Configuration;

namespace WinGetStudio.Services.DesiredStateConfiguration.Models;

internal sealed class DSCApplyUnitResult : IDSCApplyUnitResult
{
    public DSCApplyUnitResult(ApplyConfigurationUnitResult unitResult)
    {
        // Constructor copies all the required data from the out-of-proc COM
        // objects over to the current process. This ensures that we have this
        // information available even if the out-of-proc COM objects are no
        // longer available (e.g. AppInstaller service is no longer running).
        Unit = new DSCUnit(unitResult.Unit);
        State = unitResult.State;
        PreviouslyInDesiredState = unitResult.PreviouslyInDesiredState;
        RebootRequired = unitResult.RebootRequired;
        ResultInformation = unitResult.ResultInformation == null ? null : new DSCUnitResultInformation(unitResult.ResultInformation);
    }

    public IDSCUnit Unit { get; }

    public ConfigurationUnitState State{ get; }

    public bool PreviouslyInDesiredState { get; }

    public bool RebootRequired{ get; }

    public IDSCUnitResultInformation ResultInformation{ get; }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using Microsoft.Management.Configuration;

namespace WinGetStudio.Services.DesiredStateConfiguration.Models;

internal class DSCSetChangeData : IDSCSetChangeData
{
    /// <inheritdoc/>
    public ConfigurationSetChangeEventType Change { get; }

    /// <inheritdoc/>
    public IDSCUnitResultInformation ResultInformation { get; }

    /// <inheritdoc/>
    public ConfigurationSetState SetState { get; }

    /// <inheritdoc/>
    public IDSCUnit Unit { get; }

    /// <inheritdoc/>
    public ConfigurationUnitState UnitState { get; }

    public DSCSetChangeData(ConfigurationSetChangeData changeData)
    {
        Change = changeData.Change;
        ResultInformation = changeData.ResultInformation == null ? null : new DSCUnitResultInformation(changeData.ResultInformation);
        SetState = changeData.SetState;
        Unit = changeData.Unit == null ? null : new DSCUnit(changeData.Unit);
        UnitState = changeData.UnitState;
    }
}

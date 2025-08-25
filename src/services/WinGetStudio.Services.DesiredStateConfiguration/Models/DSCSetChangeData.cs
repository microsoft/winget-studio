// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Management.Configuration;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;

namespace WinGetStudio.Services.DesiredStateConfiguration.Models;

internal sealed class DSCSetChangeData : IDSCSetChangeData
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

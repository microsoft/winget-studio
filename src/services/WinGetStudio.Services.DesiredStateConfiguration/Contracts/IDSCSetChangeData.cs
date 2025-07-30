// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Management.Configuration;

namespace WinGetStudio.Services.DesiredStateConfiguration.Contracts;

public interface IDSCSetChangeData
{
    /// <summary>
    /// Gets the change event type that occurred.
    /// </summary>
    ConfigurationSetChangeEventType Change { get; }

    /// <summary>
    ///  Gets the information on the result of the attempt to apply the configuration unit.
    /// </summary>
    IDSCUnitResultInformation ResultInformation { get; }

    /// <summary>
    /// Gets the state of the configuration set for this event (the ConfigurationSet can be used to get the current state, which may be different).
    /// </summary>
    ConfigurationSetState SetState { get; }

    /// <summary>
    ///  Gets the configuration unit whose state changed.
    /// </summary>
    IDSCUnit Unit { get; }

    /// <summary>
    /// Gets the state of the configuration unit for this event (the ConfigurationUnit can be used to get the current state, which may be different).
    /// </summary>
    ConfigurationUnitState UnitState { get; }
}

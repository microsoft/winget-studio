// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Management.Configuration;

namespace WinGetStudio.Services.DesiredStateConfiguration.Contracts;

public interface IDSCApplyUnitResult
{
    /// <summary>
    /// Gets the configuration unit that was applied.
    /// </summary>
    public IDSCUnit Unit { get; }

    /// <summary>
    /// Gets the state of the configuration unit with regards to the current execution of ApplySet.
    /// </summary>
    public ConfigurationUnitState State { get; }

    /// <summary>
    /// Gets a value indicating whether or not the configuration unit was in the desired state (Test returns true) prior to the apply action.
    /// </summary>
    public bool PreviouslyInDesiredState { get; }

    /// <summary>
    /// Gets a value indicating whether the configuration unit requires a reboot.
    /// </summary>
    public bool RebootRequired { get; }

    /// <summary>
    /// Gets the result of applying the configuration unit.
    /// </summary>
    public IDSCUnitResultInformation ResultInformation { get; }
}

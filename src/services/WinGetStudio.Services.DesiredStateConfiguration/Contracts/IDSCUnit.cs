// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Management.Configuration;
using WinGetStudio.Services.DesiredStateConfiguration.Models;

namespace WinGetStudio.Services.DesiredStateConfiguration.Contracts;

public interface IDSCUnit
{
    /// <summary>
    /// Gets the type of the unit being configured
    /// </summary>
    string Type { get; }

    /// <summary>
    /// Gets the identifier name of this instance within the set.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets an identifier used to uniquely identify the instance of a configuration unit on the system.
    /// </summary>
    Guid InstanceId { get; }

    /// <summary>
    /// Gets a description of the configuration unit.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets a value indicating whether the operation requires elevated permissions to execute.
    /// </summary>
    SecurityContext SecurityContext { get; }

    /// <summary>
    /// Gets the intent of how this configuration unit will be used.
    /// </summary>
    string Intent { get; }

    /// <summary>
    /// Gets the name of the module that this configuration unit belongs to.
    /// </summary>
    public string ModuleName { get; }

    /// <summary>
    /// Gets the <see cref="Id"/> values of the configuration units that this unit depends on.
    /// </summary>
    ISet<string> Dependencies { get; }

    /// <summary>
    /// Gets the values that are for use by the configuration unit itself.
    /// </summary>
    DSCPropertySet Settings { get; }

    /// <summary>
    /// Gets the metadata properties associated with the configuration unit.
    /// </summary>
    DSCPropertySet Metadata { get; }

    /// <summary>
    /// Gets the details for this configuration unit.
    /// </summary>
    IDSCUnitDetails Details { get; }
}

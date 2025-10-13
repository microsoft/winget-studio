// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Management.Configuration;
using WinGetStudio.Services.DesiredStateConfiguration.Models;

namespace WinGetStudio.Services.DesiredStateConfiguration.Contracts;

public interface IDSCUnit
{
    /// <summary>
    /// Gets the type of the unit being configured
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// Gets the identifier name of this instance within the set.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets an identifier used to uniquely identify the instance of a configuration unit on the system.
    /// </summary>
    public Guid InstanceId { get; }

    /// <summary>
    /// Gets a description of the configuration unit.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets a value indicating whether the operation requires elevated permissions to execute.
    /// </summary>
    public SecurityContext SecurityContext { get; }

    /// <summary>
    /// Gets the intent of how this configuration unit will be used.
    /// </summary>
    public string Intent { get; }

    /// <summary>
    /// Gets the name of the module that this configuration unit belongs to.
    /// </summary>
    public string ModuleName { get; }

    /// <summary>
    /// Gets the <see cref="Id"/> values of the configuration units that this unit depends on.
    /// </summary>
    public ISet<string> Dependencies { get; }

    /// <summary>
    /// Gets the values that are for use by the configuration unit itself.
    /// </summary>
    public DSCPropertySet Settings { get; }

    /// <summary>
    /// Gets the metadata properties associated with the configuration unit.
    /// </summary>
    public DSCPropertySet Metadata { get; }

    /// <summary>
    /// Gets the details of the configuration unit.
    /// </summary>
    /// <returns>The details of the configuration unit.</returns>
    public Task<IDSCUnitDetails> GetDetailsAsync();
}

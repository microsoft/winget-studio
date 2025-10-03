// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Management.Configuration;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;

namespace WinGetStudio.Services.DesiredStateConfiguration.Models;

public class EditableDSCUnit : IDSCUnit
{
    /// <inheritdoc/>
    public string Type { get; set; }

    /// <inheritdoc/>
    public string Id { get; set; }

    /// <inheritdoc/>
    public Guid InstanceId { get; } = Guid.NewGuid();

    /// <inheritdoc/>
    public string Description { get; set; }

    /// <inheritdoc/>
    public ConfigurationUnitIntent Intent { get; set; }

    /// <inheritdoc/>
    public string ModuleName { get; set; }

    /// <inheritdoc/>
    public IList<string> Dependencies { get; set; } = [];

    /// <inheritdoc/>
    public IList<KeyValuePair<string, object>> Settings { get; set; } = [];

    /// <inheritdoc/>
    public IList<KeyValuePair<string, object>> Metadata { get; set; } = [];

    /// <inheritdoc/>
    public SecurityContext SecurityContext { get; set; }

    public EditableDSCUnit(IDSCUnit unit)
    {
        Id = unit.Id;
        ModuleName = unit.ModuleName;
        Type = unit.Type;
        Intent = unit.Intent;
        Metadata = unit.Metadata;
        Settings = unit.Settings;
        Dependencies = unit.Dependencies;
        Description = unit.Description;
        SecurityContext = unit.SecurityContext;
    }

    public EditableDSCUnit()
    {
    }

    /// <inheritdoc/>
    public Task<IDSCUnitDetails> GetDetailsAsync()
    {
        return Task.FromResult<IDSCUnitDetails>(new DSCUnitDetails(ModuleName));
    }
}

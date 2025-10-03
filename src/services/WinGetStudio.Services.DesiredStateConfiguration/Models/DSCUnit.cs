// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Management.Configuration;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;

namespace WinGetStudio.Services.DesiredStateConfiguration.Models;

internal sealed class DSCUnit : IDSCUnit
{
    private const string DescriptionMetadataKey = "description";
    private const string ModuleMetadataKey = "module";
    private readonly IDSCUnitDetails _defaultDetails;
    private Task<IDSCUnitDetails> _loadDetailsTask;

    /// <inheritdoc/>
    public string Type { get; }

    /// <inheritdoc/>
    public string Id { get; }

    /// <inheritdoc/>
    public Guid InstanceId { get; }

    /// <inheritdoc/>
    public string Description { get; }

    /// <inheritdoc/>
    public SecurityContext SecurityContext { get; }

    /// <inheritdoc/>
    public ConfigurationUnitIntent Intent { get; }

    /// <inheritdoc/>
    public string ModuleName { get; }

    /// <inheritdoc/>
    public IList<string> Dependencies { get; }

    /// <inheritdoc/>
    public IList<KeyValuePair<string, object>> Settings { get; }

    /// <inheritdoc/>
    public IList<KeyValuePair<string, object>> Metadata { get; }

    internal ConfigurationUnit ConfigUnit { get; }

    public DSCUnit(ConfigurationUnit unit)
    {
        ConfigUnit = unit;

        // Constructor copies all the required data from the out-of-proc COM
        // objects over to the current process. This ensures that we have this
        // information available even if the out-of-proc COM objects are no
        // longer available (e.g. AppInstaller service is no longer running).
        Type = unit.Type;
        Id = unit.Identifier;
        InstanceId = unit.InstanceIdentifier;
        Intent = unit.Intent;
        Dependencies = [.. unit.Dependencies];

        // Get description from settings
        unit.Metadata.TryGetValue(DescriptionMetadataKey, out var descriptionObj);
        Description = descriptionObj?.ToString() ?? string.Empty;

        // Get security context
        SecurityContext = GetSecurityContext(unit);

        // Load dictionary values into list of key value pairs
        // TODO: Maybe deep copy?
        Settings = unit.Settings.Select(s => new KeyValuePair<string, object>(s.Key, s.Value)).ToList();
        Metadata = unit.Metadata.Select(m => new KeyValuePair<string, object>(m.Key, m.Value)).ToList();

        // Get module name from metadata
        ModuleName = Metadata.FirstOrDefault(m => m.Key == ModuleMetadataKey).Value?.ToString() ?? string.Empty;

        // Build default details
        _defaultDetails = unit.Details == null ? new DSCUnitDetails(ModuleName) : new DSCUnitDetails(unit.Details);
        _loadDetailsTask = Task.FromResult(_defaultDetails);
    }

    /// <inheritdoc/>
    public async Task<IDSCUnitDetails> GetDetailsAsync()
    {
        var loadedDetails = await _loadDetailsTask;
        return loadedDetails ?? _defaultDetails;
    }

    /// <summary>
    /// Set an asynchronous task to load the details for the unit in the background.
    /// </summary>
    /// <param name="loadDetailsTask">Task to load the details</param>
    internal void SetLoadDetailsTask(Task<IDSCUnitDetails> loadDetailsTask)
    {
        _loadDetailsTask = loadDetailsTask;
    }

    /// <summary>
    /// Gets a value indicating whether the configuration unit requires elevated permissions to execute.
    /// </summary>
    /// <param name="unit">ConfigurationUnit unit</param>
    /// <returns>True if the unit requires elevation, false otherwise.</returns>
    public SecurityContext GetSecurityContext(ConfigurationUnit unit)
    {
        // This property is not available in older version of winget.
        try
        {
            return unit.Environment.Context;
        }
        catch
        {
            // If we cannot determine the security context, return default
            return default;
        }
    }
}

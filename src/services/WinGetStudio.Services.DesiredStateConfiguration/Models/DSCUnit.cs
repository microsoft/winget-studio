// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Management.Configuration;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;

namespace WinGetStudio.Services.DesiredStateConfiguration.Models;

internal sealed class DSCUnit : IDSCUnit
{
    private const string DescriptionMetadataKey = "description";
    private const string ModuleMetadataKey = "module";

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
    public string Intent { get; }

    /// <inheritdoc/>
    public string ModuleName { get; }

    /// <inheritdoc/>
    public ISet<string> Dependencies { get; }

    /// <inheritdoc/>
    public DSCPropertySet Settings { get; }

    /// <inheritdoc/>
    public DSCPropertySet Metadata { get; }

    /// <inheritdoc/>
    public IDSCUnitDetails Details { get; }

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
        Intent = unit.Intent.ToString();
        Dependencies = unit.Dependencies.ToHashSet();

        // Get description from settings
        unit.Metadata.TryGetValue(DescriptionMetadataKey, out var descriptionObj);
        Description = descriptionObj?.ToString() ?? string.Empty;

        // Get security context
        SecurityContext = GetSecurityContext(unit);

        // Load dictionary values into list of key value pairs
        Settings = new(unit.Settings);
        Metadata = new(unit.Metadata);

        // Get module name from metadata
        ModuleName = Metadata.FirstOrDefault(m => m.Key == ModuleMetadataKey).Value?.ToString() ?? string.Empty;

        // Get details if available
        Details = unit.Details == null ? null : new DSCUnitDetails(unit.Details);
    }

    /// <summary>
    /// Gets the security context for the unit.
    /// </summary>
    /// <param name="unit">ConfigurationUnit unit</param>
    /// <returns>The security context.</returns>
    public SecurityContext GetSecurityContext(ConfigurationUnit unit)
    {
        // This property is not available in older version of winget.
        try
        {
            return unit.Environment.Context;
        }
        catch
        {
            // If we cannot determine the security context, return default option.
            return default;
        }
    }
}

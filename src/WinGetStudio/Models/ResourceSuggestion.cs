// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

namespace WinGetStudio.Models;

/// <summary>
/// Represents a suggestion for a DSC resource.
/// </summary>
public sealed partial class ResourceSuggestion
{
    /// <summary>
    /// Gets the module containing the resource.
    /// </summary>
    public DSCModule Module { get; }

    /// <summary>
    /// Gets the suggested DSC resource.
    /// </summary>
    public DSCResource Resource { get; }

    /// <summary>
    /// Gets the module identifier.
    /// </summary>
    public string ModuleId => Module.Id;

    /// <summary>
    /// Gets the resource name.
    /// </summary>
    public string ResourceName => Resource.Name;

    /// <summary>
    /// Gets the resource version.
    /// </summary>
    public string Version => Resource.Version;

    /// <summary>
    /// Gets the DSC version of the resource.
    /// </summary>
    public DSCVersion DSCVersion => Resource.DSCVersion;

    /// <summary>
    /// Gets the source of the module.
    /// </summary>
    public string Source => Module.Source.ToString();

    /// <summary>
    /// Gets a value indicating whether the module is virtual.
    /// </summary>
    public bool IsModuleVirtual => Module.IsVirtual;

    public ResourceSuggestion(DSCModule module, DSCResource resource)
    {
        Module = module;
        Resource = resource;
    }
}

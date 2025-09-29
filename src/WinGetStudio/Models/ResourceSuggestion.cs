// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

namespace WinGetStudio.Models;

public sealed partial class ResourceSuggestion
{
    public DSCModule Module { get; }

    public DSCResource Resource { get; }

    public string ModuleId => Module.Id;

    public string ResourceName => Resource.Name;

    public string Version => Resource.Version;

    public DSCVersion DSCVersion => Resource.DSCVersion;

    public string Source => Module.Source.ToString();

    public bool IsModuleVirtual => Module.IsVirtual;

    public ResourceSuggestion(DSCModule module, DSCResource resource)
    {
        Module = module;
        Resource = resource;
    }
}

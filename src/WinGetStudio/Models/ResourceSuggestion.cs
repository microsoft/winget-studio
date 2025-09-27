// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

namespace WinGetStudio.Models;

public sealed partial class ResourceSuggestion
{
    public string ModuleId { get; }

    public string ResourceName { get; }

    public string Version { get; }

    public DSCVersion DSCVersion { get; }

    public string Source { get; }

    public bool IsModuleVirtual { get; }

    public ResourceSuggestion(DSCModule module, DSCResource resource)
    {
        ModuleId = module.Id;
        ResourceName = resource.Name;
        Version = resource.Version;
        DSCVersion = resource.DSCVersion;
        Source = module.Source.ToString();
        IsModuleVirtual = module.IsVirtual;
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuGet.Versioning;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

internal sealed class DSCModule : IDSCModule
{
    public bool IsLocal { get; set; }

    public int DscVersion { get; set; }

    public string Id { get; set; }

    public NuGetVersion Version { get; set; }

    public string Tags { get; set; }

    public IModuleProvider Provider { get; set; }

    public Dictionary<string, DSCResource> Resources { get; set; } = [];

    public DSCModule(IModuleProvider provider, string id, string version, string tags)
    {
        Provider = provider;
        Id = id;
        Version = new NuGetVersion(version);
        Tags = tags;
    }

    public async Task LoadDSCResourcesAsync()
    {
        var resourceNames = await Provider.GetDscModuleResourcesAsync(this);
        Resources = resourceNames.ToDictionary(name => name, name => new DSCResource() { Name = name });
    }

    public async Task LoadDSCResourcePropertiesAsync()
    {
        await Task.CompletedTask;
    }
}

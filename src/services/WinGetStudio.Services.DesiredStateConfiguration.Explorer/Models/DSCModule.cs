// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using NuGet.Versioning;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

internal sealed class DSCModule : IDSCModule
{
    public string Name { get; }

    public NuGetVersion Version { get; }

    public string Tags { get; }

    public IModuleProvider Provider { get; }

    public DSCModule(IModuleProvider provider, string name, string version, string tags)
    {
        Provider = provider;
        Name = name;
        Version = new NuGetVersion(version);
        Tags = tags;
    }

    public Task<IReadOnlyList<string>> GetDSCResourcesAsync()
    {
        return Provider.GetDscModuleResourcesAsync(this);
    }
}

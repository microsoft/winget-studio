// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using WinGetStudio.CLI.DSCv3.Contracts;
using WinGetStudio.CLI.DSCv3.DscResources;

namespace WinGetStudio.CLI.DSCv3.Services;

internal sealed class ResourceProvider : IResourceProvider
{
    private readonly IServiceProvider _sp;
    private readonly Dictionary<string, Func<BaseResource>> _resourceFactories;

    public ResourceProvider(IServiceProvider sp)
    {
        _sp = sp;
        _resourceFactories = new()
        {
            { SettingsResource.ResourceName, () => _sp.GetRequiredService<SettingsResource>() },

            // Add other resources here
        };
    }

    /// <inheritdoc/>
    public IEnumerable<string> ResourceNames => _resourceFactories.Keys;

    /// <inheritdoc/>
    public bool IsResourceAvailable(string resourceName)
    {
        return _resourceFactories.ContainsKey(resourceName);
    }

    /// <inheritdoc/>
    public BaseResource GetResource(string name)
    {
        Debug.Assert(IsResourceAvailable(name), $"Resource '{name}' is not available.");
        return _resourceFactories[name]();
    }
}

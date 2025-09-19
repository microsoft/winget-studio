// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Services;

internal sealed class DSCExplorer : IDSCExplorer
{
    private readonly IEnumerable<IModuleProvider> _moduleProviders;

    public DSCExplorer(IEnumerable<IModuleProvider> moduleProviders)
    {
        _moduleProviders = moduleProviders;
    }

    public async Task<IReadOnlyList<IDSCModule>> GetDSCModulesAsync()
    {
        List<IDSCModule> allModules = new();
        foreach (var provider in _moduleProviders)
        {
            var modules = await provider.GetDSCModulesAsync();
            allModules.AddRange(modules);
        }

        return allModules;
    }

    public Task<IReadOnlyList<IDSCModule>> GetDSCModulesAsync<TModuleProvider>()
        where TModuleProvider : IModuleProvider
    {
        Debug.Assert(_moduleProviders.OfType<TModuleProvider>().Any(), $"No module providers of type {typeof(TModuleProvider).FullName} are registered.");
        var provider = _moduleProviders.OfType<TModuleProvider>().FirstOrDefault();
        return provider.GetDSCModulesAsync();
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Services;

internal sealed class DSCExplorer : IDSCExplorer
{
    private readonly IEnumerable<IModuleProvider> _moduleProviders;

    public DSCExplorer(IEnumerable<IModuleProvider> moduleProviders)
    {
        _moduleProviders = moduleProviders;
    }

    /// <inheritdoc/>
    public async Task<DSCModuleCatalog> GetCatalogAsync<TModuleProvider>()
        where TModuleProvider : IModuleProvider
    {
        Debug.Assert(_moduleProviders.OfType<TModuleProvider>().Any(), $"No module provider of type {typeof(TModuleProvider).FullName} is registered.");
        return await _moduleProviders.OfType<TModuleProvider>().First().GetModuleCatalogAsync();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<DSCModuleCatalog>> GetCatalogsAsync()
    {
        List<DSCModuleCatalog> catalogs = [];
        foreach (var provider in _moduleProviders)
        {
            catalogs.Add(await provider.GetModuleCatalogAsync());
        }

        return catalogs;
    }
}

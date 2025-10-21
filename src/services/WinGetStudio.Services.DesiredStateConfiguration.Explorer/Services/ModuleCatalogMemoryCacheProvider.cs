// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Services;

internal sealed class ModuleCatalogMemoryCacheProvider : IModuleCatalogMemoryCacheProvider
{
    private readonly ConcurrentDictionary<string, DSCModuleCatalog> _catalogCache = [];

    /// <inheritdoc/>
    public bool TryGet(string catalogName, out DSCModuleCatalog catalog) => _catalogCache.TryGetValue(catalogName, out catalog);

    /// <inheritdoc/>
    public void Set(DSCModuleCatalog catalog) => _catalogCache[catalog.Name] = catalog;

    /// <inheritdoc/>
    public void Remove(string catalogName) => _catalogCache.TryRemove(catalogName, out _);
}

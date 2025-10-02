// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;

internal interface IModuleCatalogMemoryCacheProvider
{
    /// <summary>
    /// Tries to get a DSC module catalog from the cache by its name.
    /// </summary>
    /// <param name="catalogName">The name of the catalog to retrieve.</param>
    /// <param name="catalog">The retrieved DSC module catalog, if found; otherwise, null.</param>
    /// <returns>>True if the catalog was found in the cache; otherwise, false.</returns>
    bool TryGet(string catalogName, out DSCModuleCatalog catalog);

    /// <summary>
    /// Sets the specified module catalog for the given catalog name.
    /// </summary>
    /// <param name="catalog">The module catalog to set.</param>
    void Set(DSCModuleCatalog catalog);

    /// <summary>
    /// Removes the module catalog associated with the specified catalog name from the cache.
    /// </summary>
    /// <param name="catalogName">The name of the catalog to remove from the cache.</param>
    void Remove(string catalogName);
}

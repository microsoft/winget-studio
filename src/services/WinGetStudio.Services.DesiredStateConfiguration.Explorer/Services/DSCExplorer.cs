// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Services;

internal sealed class DSCExplorer : IDSCExplorer
{
    private readonly IModuleCatalogRepository _repository;

    public DSCExplorer(IModuleCatalogRepository repository)
    {
        _repository = repository;
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<DSCModuleCatalog> GetModuleCatalogsAsync()
    {
        return _repository.GetModuleCatalogsAsync();
    }

    /// <inheritdoc/>
    public async Task EnrichModuleWithResourceDetailsAsync(DSCModule dscModule)
    {
        await _repository.EnrichModuleWithResourceDetailsAsync(dscModule);
    }

    /// <inheritdoc/>
    public async Task ClearCacheAsync()
    {
        await _repository.ClearCacheAsync();
    }
}

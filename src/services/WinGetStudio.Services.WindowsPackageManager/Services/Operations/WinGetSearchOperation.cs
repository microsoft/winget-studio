// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WinGetStudio.Services.WindowsPackageManager.Contracts;
using WinGetStudio.Services.WindowsPackageManager.Contracts.Operations;
using WinGetStudio.Services.WindowsPackageManager.Models;

namespace WinGetStudio.Services.WindowsPackageManager.Services.Operations;

/// <summary>
/// Search for packages using WinGet with recovery
/// </summary>
internal sealed class WinGetSearchOperation : IWinGetSearchOperation
{
    private readonly ILogger _logger;
    private readonly IWinGetCatalogConnector _catalogConnector;
    private readonly IWinGetPackageFinder _packageFinder;
    private readonly IWinGetRecovery _recovery;

    public WinGetSearchOperation(
        ILogger<WinGetSearchOperation> logger,
        IWinGetCatalogConnector catalogConnector,
        IWinGetPackageFinder packageFinder,
        IWinGetRecovery recovery)
    {
        _logger = logger;
        _catalogConnector = catalogConnector;
        _packageFinder = packageFinder;
        _recovery = recovery;
    }

    /// <inheritdoc />
    public async Task<IList<IWinGetPackage>> SearchAsync(string query, uint limit)
    {
        return await _recovery.DoWithRecoveryAsync(async () =>
        {
            var searchCatalog = await _catalogConnector.GetCustomSearchCatalogAsync();
            var results = await _packageFinder.SearchAsync(searchCatalog, query, limit);
            return WinGetPackage.FromOutOfProc(_logger, _packageFinder, results);
        });
    }
}

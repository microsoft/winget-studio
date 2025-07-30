// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using WinGetStudio.Services.WindowsPackageManager.Contracts;
using WinGetStudio.Services.WindowsPackageManager.Contracts.Operations;
using WinGetStudio.Services.WindowsPackageManager.Models;

namespace WinGetStudio.Services.WindowsPackageManager.Services.Operations;

/// <summary>
/// Install packages using WinGet with recovery
/// </summary>
internal sealed class WinGetInstallOperation : IWinGetInstallOperation
{
    private readonly IWinGetPackageInstaller _packageInstaller;
    private readonly IWinGetProtocolParser _protocolParser;
    private readonly IWinGetCatalogConnector _catalogConnector;
    private readonly IWinGetRecovery _recovery;

    public WinGetInstallOperation(
        IWinGetPackageInstaller packageInstaller,
        IWinGetProtocolParser protocolParser,
        IWinGetCatalogConnector catalogConnector,
        IWinGetRecovery recovery)
    {
        _packageInstaller = packageInstaller;
        _protocolParser = protocolParser;
        _catalogConnector = catalogConnector;
        _recovery = recovery;
    }

    /// <inheritdoc />
    public async Task<IWinGetInstallPackageResult> InstallPackageAsync(WinGetPackageUri packageUri, Guid activityId)
    {
        return await _recovery.DoWithRecoveryAsync(async () =>
        {
            var catalog = await _protocolParser.ResolveCatalogAsync(packageUri);
            return await _packageInstaller.InstallPackageAsync(catalog, packageUri.PackageId, packageUri.Options.Version, activityId);
        });
    }
}

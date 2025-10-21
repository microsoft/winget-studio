// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Services;

public sealed partial class NuGetDownloader : INuGetDownloader
{
    private readonly ILogger<NuGetDownloader> _logger;

    public NuGetDownloader(ILogger<NuGetDownloader> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public SourceRepository CreateRepositoryCoreV2(string sourceUrl)
    {
        return Repository.Factory.GetCoreV2(new PackageSource(sourceUrl));
    }

    /// <inheritdoc/>
    public SourceRepository CreateRepositoryCoreV3(string sourceUrl)
    {
        return Repository.Factory.GetCoreV3(new PackageSource(sourceUrl));
    }

    /// <inheritdoc/>
    public async Task<OpenPackageResult> OpenPackageAsync(SourceRepository repository, string id, NuGetVersion version)
    {
        using var cache = new SourceCacheContext();
        var download = await repository.GetResourceAsync<DownloadResource>();
        var result = await download.GetDownloadResourceResultAsync(
            new PackageIdentity(id, version),
            new PackageDownloadContext(cache),
            Path.GetTempPath(),
            NullLogger.Instance,
            CancellationToken.None);

        _logger.LogInformation($"Attempted to download package {id} v{version} from {repository.PackageSource.Source}; result = {result.Status}");

        if (result.Status != DownloadResourceResultStatus.Available)
        {
            result.Dispose();
            return new OpenPackageResult
            {
                Success = false,
            };
        }

        return new OpenPackageResult
        {
            Success = true,
            PackageStream = result.PackageStream,
        };
    }
}

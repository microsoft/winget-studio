// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Logging;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Services;

internal sealed class PowerShellGalleryModuleProvider : IModuleProvider
{
    // Base URL for PowerShell Gallery search API
    private const string BaseUrl = "https://www.powershellgallery.com/api/v2";

    // Page size for each API request. Maximum allowed is 100.
    private const int PageSize = 100;

    private readonly INuGetV2Client _client;
    private readonly INuGetDownloader _downloader;
    private readonly IPsm1Parser _psm1Parser;
    private readonly ILogger<PowerShellGalleryModuleProvider> _logger;
    private readonly SourceRepository _repository;

    public PowerShellGalleryModuleProvider(
        ILogger<PowerShellGalleryModuleProvider> logger,
        INuGetV2Client client,
        INuGetDownloader downloader,
        IPsm1Parser psm1Parser)
    {
        _logger = logger;
        _client = client;
        _downloader = downloader;
        _psm1Parser = psm1Parser;
        _repository = _downloader.CreateRepositoryCoreV2(BaseUrl);
    }

    /// <inheritdoc/>
    public string Name => "PSGallery";

    /// <inheritdoc />
    public async Task<DSCModuleCatalog> GetModuleCatalogAsync()
    {
        var catalog = new DSCModuleCatalog() { Name = Name };
        try
        {
            List<DSCModule> modules = [];
            var hasMore = true;
            var skip = 0;
            while (hasMore)
            {
                var batch = await _client.SearchAsync(BaseUrl, BuildQuery(skip));
                var batchSize = batch.Count;
                modules.AddRange(batch.Select(metadata => new DSCModule(this)
                {
                    Id = metadata.Id,
                    Version = metadata.Version,
                    Tags = metadata.Tags,
                }));

                if (batchSize < PageSize)
                {
                    hasMore = false;
                }
                else
                {
                    skip += PageSize;
                }
            }

            catalog.Modules = modules;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception occurred while retrieving DSC modules from PowerShell Gallery.");
        }

        return catalog;
    }

    /// <inheritdoc/>
    public Task<IReadOnlySet<string>> GetResourceNamesAsync(DSCModule dscModule)
    {
        // For powershell gallery modules, we extract resource names from tags
        var dscResourcePrefix = "PSDscResource_";
        var tags = dscModule.Tags.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var resourceTags = tags.Where(tag => tag.StartsWith(dscResourcePrefix, StringComparison.Ordinal));
        var resourceNames = resourceTags.Select(r => r[dscResourcePrefix.Length..]).ToList();
        return Task.FromResult<IReadOnlySet<string>>(resourceNames.ToHashSet());
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<DSCResourceClassDefinition>> GetResourceDefinitionsAsync(DSCModule dscModule)
    {
        List<DSCResourceClassDefinition> resources = [];
        var openResult = await _downloader.OpenPackageAsync(_repository, dscModule.Id, NuGetVersion.Parse(dscModule.Version));
        if (openResult.Success)
        {
            using var stream = openResult.PackageStream;
            using var zip = new ZipArchive(stream, ZipArchiveMode.Read);
            var psm1Entries = zip.Entries.Where(e => e.FullName.EndsWith(".psm1", StringComparison.OrdinalIgnoreCase));
            foreach (var psm1Entry in psm1Entries)
            {
                using var sr = new StreamReader(psm1Entry.Open(), Encoding.UTF8);
                var psm1Content = await sr.ReadToEndAsync();
                var dscResources = _psm1Parser.ParseDscResources(psm1Content);
                resources.AddRange(dscResources);
            }
        }

        return resources;
    }

    /// <summary>
    /// Builds the query part of the URL for searching DSC modules.
    /// </summary>
    /// <param name="skip">Number of records to skip for pagination.</param>
    /// <returns>The query part of the URL.</returns>
    private NameValueCollection BuildQuery(int skip)
    {
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["includePrerelease"] = "true";
        query["$skip"] = $"{skip}";
        query["$top"] = $"{PageSize}";
        query["$filter"] = string.Join(" and ", [
            "IsAbsoluteLatestVersion eq true",
            "substringof('dscresource', tolower(Tags))",
        ]);
        return query;
    }
}

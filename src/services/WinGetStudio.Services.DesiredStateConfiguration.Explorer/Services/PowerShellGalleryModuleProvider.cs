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
    private const string BaseUrl = "https://www.powershellgallery.com/api/v2";
    private const int PageSize = 100;
    private const string DscResourceTagPrefix = "PSDscResource_";

    private readonly INuGetV2Client _client;
    private readonly INuGetDownloader _downloader;
    private readonly IEnumerable<IDSCResourceParser> _parsers;
    private readonly ILogger<PowerShellGalleryModuleProvider> _logger;
    private readonly SourceRepository _repository;

    public PowerShellGalleryModuleProvider(
        ILogger<PowerShellGalleryModuleProvider> logger,
        INuGetV2Client client,
        INuGetDownloader downloader,
        IEnumerable<IDSCResourceParser> parsers)
    {
        _logger = logger;
        _client = client;
        _downloader = downloader;
        _parsers = parsers;
        _repository = _downloader.CreateRepositoryCoreV2(BaseUrl);
    }

    /// <inheritdoc/>
    public string Name => "PSGallery";

    /// <inheritdoc />
    public async Task<DSCModuleCatalog> GetModuleCatalogAsync()
    {
        var catalog = new DSCModuleCatalog { Name = Name };
        try
        {
            var modules = new List<DSCModule>();
            for (var skip = 0; ; skip += PageSize)
            {
                var query = BuildQuery(skip);
                var batch = await _client.SearchAsync(BaseUrl, query);
                modules.AddRange(batch.Select(metadata => new DSCModule(this)
                {
                    Id = metadata.Id,
                    Version = metadata.Version,
                    Tags = metadata.Tags,
                }));

                if (batch.Count < PageSize)
                {
                    break;
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
        var tags = dscModule.Tags.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var resourceTags = tags.Where(tag => tag.StartsWith(DscResourceTagPrefix, StringComparison.Ordinal));
        var resourceNames = resourceTags.Select(r => r[DscResourceTagPrefix.Length..]).ToList();
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
            foreach (var entry in zip.Entries)
            {
                var parsers = _parsers.Where(p => p.CanParse(entry.FullName));
                if (parsers.Any())
                {
                    using var sr = new StreamReader(entry.Open(), Encoding.UTF8);
                    foreach (var parser in parsers)
                    {
                        var parsedResources = await parser.ParseAsync(sr);
                        resources.AddRange(parsedResources);
                    }
                }
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

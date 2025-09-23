// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management.Automation.Language;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Logging;
using NuGet.Common;
using NuGet.Packaging.Core;
using NuGet.Protocol;
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
    private readonly ILogger<PowerShellGalleryModuleProvider> _logger;
    private readonly SourceRepository _repository;

    public PowerShellGalleryModuleProvider(ILogger<PowerShellGalleryModuleProvider> logger, INuGetV2Client client)
    {
        _logger = logger;
        _repository = Repository.Factory.GetCoreV3(BaseUrl);
        _client = client;
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

    /// <inheritdoc/>
    public async Task<IReadOnlyList<DSCResourceClassDefinition>> GetResourceDefinitionsAsync(DSCModule dscModule)
    {
        var identity = new PackageIdentity(dscModule.Id, NuGetVersion.Parse(dscModule.Version));
        var tempPath = Path.GetTempPath();
        using var cache = new SourceCacheContext();
        var downloadContext = new PackageDownloadContext(cache);

        var download = await _repository.GetResourceAsync<DownloadResource>();
        using var result = await download.GetDownloadResourceResultAsync(identity, downloadContext, tempPath, NullLogger.Instance, CancellationToken.None);
        if (result.Status == DownloadResourceResultStatus.NotFound)
        {
            _logger.LogError($"Module {dscModule.Id} version {dscModule.Version} not found in PowerShell Gallery.");
            return [];
        }

        if (result.Status == DownloadResourceResultStatus.Cancelled)
        {
            _logger.LogWarning($"Download of module {dscModule.Id} version {dscModule.Version} was cancelled.");
            return [];
        }

        if (result.Status == DownloadResourceResultStatus.AvailableWithoutStream)
        {
            _logger.LogDebug($"Module {dscModule.Id} version {dscModule.Version} is available without stream.");
        }

        using var zip = new ZipArchive(result.PackageStream, ZipArchiveMode.Read, leaveOpen: true);
        var psm1Entries = zip.Entries.Where(e => e.FullName.EndsWith(".psm1", StringComparison.OrdinalIgnoreCase));

        // TODO Add support for MOF entries as well handling CIM-based resources
        List<DSCResourceClassDefinition> resources = [];
        foreach (var psm1Entry in psm1Entries)
        {
            using var sr = new StreamReader(psm1Entry.Open(), Encoding.UTF8);
            var psm1Content = await sr.ReadToEndAsync();
            resources.AddRange(ParseClasses(psm1Content));
        }

        return resources;
    }

    private List<DSCResourceClassDefinition> ParseClasses(string psm1Content)
    {
        var ast = Parser.ParseInput(psm1Content, out var tokens, out var errors);

        var classAsts = ast
            .FindAll(
                x => x is TypeDefinitionAst t
                && t.Attributes.Any(a => a.TypeName.Name.Equals("DscResource", StringComparison.OrdinalIgnoreCase)),
                true);

        List<DSCResourceClassDefinition> resources = [];
        foreach (var cls in classAsts.Cast<TypeDefinitionAst>())
        {
            var properties = cls.Members
                .OfType<PropertyMemberAst>()
                .Where(p => p.Attributes.Any(a => a.TypeName.Name.Equals("DscProperty", StringComparison.OrdinalIgnoreCase)))
                .ToList();

            resources.Add(new DSCResourceClassDefinition()
            {
                ClassAst = cls,
                Properties = properties,
            });
        }

        return resources;
    }
}

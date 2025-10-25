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
using NJsonSchema;
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

    /// <inheritdoc/>
    public string Name => nameof(DSCModuleSource.PSGallery);

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

    /// <inheritdoc />
    public async Task<GetModuleCatalogResult> GetModuleCatalogAsync()
    {
        var catalog = new DSCModuleCatalog { Name = Name };
        try
        {
            var dscModules = new Dictionary<string, DSCModule>();
            for (var skip = 0; ; skip += PageSize)
            {
                var query = BuildQuery(skip);
                var batch = await _client.SearchAsync(BaseUrl, query);
                foreach (var moduleMetadata in batch)
                {
                    // Prepare the module entry
                    var dscModule = new DSCModule
                    {
                        Id = moduleMetadata.Id,
                        Version = moduleMetadata.Version,
                        Source = DSCModuleSource.PSGallery,
                        IsVirtual = false,
                    };

                    // Populate resources
                    var resourceNames = GetResourceNamesFromTags(moduleMetadata);
                    foreach (var resourceName in resourceNames)
                    {
                        if (dscModule.AddResource(resourceName, DSCVersion.Unknown))
                        {
                            _logger.LogInformation($"Added resource '{resourceName}' to module '{dscModule.Id}' from tags.");
                        }
                        else
                        {
                            _logger.LogWarning($"Resource '{resourceName}' already exists in module '{dscModule.Id}'. Skipping addition from tags.");
                        }
                    }

                    // Add the module to the list
                    dscModules.TryAdd(dscModule.Id, dscModule);
                }

                if (batch.Count < PageSize)
                {
                    break;
                }
            }

            catalog.Modules = dscModules;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception occurred while retrieving DSC modules from PowerShell Gallery.");
        }

        return new()
        {
            CanCache = true,
            Catalog = catalog,
        };
    }

    /// <inheritdoc/>
    public async Task EnrichModuleWithResourceDetailsAsync(DSCModule dscModule)
    {
        if (!dscModule.IsEnriched)
        {
            dscModule.IsEnriched = true;
            var definitions = await GetResourceDefinitionsAsync(dscModule);
            foreach (var definition in definitions)
            {
                if (dscModule.EnrichResource(definition.ClassName, definition))
                {
                    _logger.LogInformation($"Enriched module '{dscModule.Id}' with resource details for resource '{definition.ClassName}'.");
                }
                else
                {
                    _logger.LogWarning($"Resource '{definition.ClassName}' not found in module '{dscModule.Id}' during enrichment.");
                }
            }
        }
    }

    /// <inheritdoc/>
    public Task<JsonSchema> GetResourceSchemaAsync(DSCResource resource)
    {
        // For PowerShell resources, we generate a basic schema where all properties
        // are set to type 'none'. In the future, we may enhance this by inferring
        // property types for more detailed schemas.
        var schema = new JsonSchema { Type = JsonObjectType.Object };
        foreach (var property in resource.Properties)
        {
            schema.Properties[property.Name] = new JsonSchemaProperty
            {
                Type = JsonObjectType.None,
            };
        }

        return Task.FromResult(schema);
    }

    /// <summary>
    /// Gets the resource definitions for the specified DSC module.
    /// </summary>
    /// <param name="dscModule">The DSC module to get resource definitions for.</param>
    /// <returns>>A list of DSC resource definitions.</returns>
    private async Task<IReadOnlyList<DSCResourceClassDefinition>> GetResourceDefinitionsAsync(DSCModule dscModule)
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

    /// <summary>
    /// Gets resource names from module tags.
    /// </summary>
    /// <param name="moduleMetadata">The module metadata.</param>
    /// <returns>A set of resource names.</returns>
    private HashSet<string> GetResourceNamesFromTags(ModuleMetadata moduleMetadata)
    {
        // For powershell gallery modules, we extract resource names from tags
        var tags = moduleMetadata.Tags.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var resourceTags = tags.Where(tag => tag.StartsWith(DscResourceTagPrefix, StringComparison.Ordinal));
        return [.. resourceTags.Select(r => r[DscResourceTagPrefix.Length..])];
    }
}

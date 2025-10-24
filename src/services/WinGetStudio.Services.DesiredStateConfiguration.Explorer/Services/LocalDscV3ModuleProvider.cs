// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NJsonSchema;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Services;

internal sealed class LocalDscV3ModuleProvider : IModuleProvider
{
    private readonly ILogger<LocalDscV3ModuleProvider> _logger;
    private readonly IDSC _dsc;
    private readonly IDSCProcess _dscProcess;
    private readonly IDSCResourceJsonSchemaDefaultGenerator _generator;

    /// <inheritdoc/>
    public string Name => nameof(DSCModuleSource.LocalDscV3);

    public LocalDscV3ModuleProvider(
        ILogger<LocalDscV3ModuleProvider> logger,
        IDSC dsc,
        IDSCProcess dscProcess,
        IDSCResourceJsonSchemaDefaultGenerator generator)
    {
        _logger = logger;
        _dsc = dsc;
        _dscProcess = dscProcess;
        _generator = generator;
    }

    /// <inheritdoc/>
    public async Task EnrichModuleWithResourceDetailsAsync(DSCModule dscModule)
    {
        if (!dscModule.IsEnriched)
        {
            if (dscModule.Resources?.Count == 1)
            {
                var resource = dscModule.Resources.Values.First();
                var schema = await GetResourceSchemaAsync(resource.Name);
                if (schema != null)
                {
                    dscModule.PopulateResourceFromSchema(resource.Name, schema, DSCVersion.V3, DSCModuleSource.LocalDscV3);
                    dscModule.IsEnriched = true;
                }
            }
            else
            {
                _logger.LogWarning($"Module '{dscModule.Id}' does not have exactly one resource. Instead it has {dscModule.Resources?.Count ?? 0} resources. Skipping enrichment.");
            }
        }
    }

    /// <inheritdoc/>
    public Task<JsonSchema> GetResourceSchemaAsync(DSCResource resource)
    {
        return JsonSchema.FromJsonAsync(resource.Syntax);
    }

    /// <inheritdoc/>
    public async Task<GetModuleCatalogResult> GetModuleCatalogAsync()
    {
        var catalog = new DSCModuleCatalog { Name = Name };
        var resources = await _dsc.GetDscV3ResourcesAsync();
        foreach (var resource in resources)
        {
            var module = new DSCModule
            {
                Id = resource.Name,
                Version = resource.Version,
                Source = DSCModuleSource.LocalDscV3,
                IsVirtual = true,
            };
            module.PopulateResources([resource.Name], DSCVersion.V3, DSCModuleSource.LocalDscV3);
            catalog.Modules.TryAdd(resource.Name, module);
        }

        // For local DSC v3 resources, caching is only enabled if resources are
        // discovered. This workaround addresses a known issue where the WinGet
        // COM API may return an empty resource list.
        // See: https://github.com/PowerShell/DSC/issues/786
        var cache = catalog.Modules?.Count > 0;

        return new()
        {
            CanCache = cache,
            Catalog = catalog,
        };
    }

    private async Task<JsonSchema> GetResourceSchemaAsync(string resourceName)
    {
        try
        {
            var result = await _dscProcess.GetResourceSchemaAsync(resourceName);
            if (result.IsSuccess)
            {
                return await JsonSchema.FromJsonAsync(result.Output);
            }

            _logger.LogError($"Failed to get resource schema for {resourceName}. Error: {result.Errors}");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Exception occurred while getting resource schema for {resourceName}");
            return null;
        }
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management.Automation.Language;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
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
    private const string SearchUrl = $"{BaseUrl}/Search()";

    private readonly ILogger<PowerShellGalleryModuleProvider> _logger;
    private readonly SourceRepository _repository;

    // Namespaces for parsing the PowerShell Gallery XML response
    private readonly XNamespace _atom;
    private readonly XNamespace _metadata;
    private readonly XNamespace _dataServices;

    private string DSCModulesUrl => BuildDscModuleUrl();

    public PowerShellGalleryModuleProvider(ILogger<PowerShellGalleryModuleProvider> logger)
    {
        _logger = logger;

        _repository = Repository.Factory.GetCoreV3("https://www.powershellgallery.com/api/v2");
        _atom = XNamespace.Get("http://www.w3.org/2005/Atom");
        _metadata = XNamespace.Get("http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
        _dataServices = XNamespace.Get("http://schemas.microsoft.com/ado/2007/08/dataservices");
    }

    /// <inheritdoc/>
    public string Name => "PowerShell Gallery";

    /// <inheritdoc />
    public async Task<IReadOnlyList<IDSCModule>> GetDSCModulesAsync()
    {
        List<DSCModule> modules = [];
        using (var client = new HttpClient())
        {
            try
            {
                var response = await client.GetAsync(DSCModulesUrl);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var xml = XDocument.Parse(result);
                    var entries = xml.Descendants(_atom + "entry");
                    foreach (var entry in entries)
                    {
                        var props = entry.Descendants(_metadata + "properties").FirstOrDefault();
                        var id = props.Element(_dataServices + "Id").Value;
                        var version = props.Element(_dataServices + "Version").Value;
                        var tags = props.Element(_dataServices + "Tags").Value;
                        modules.Add(new(this)
                        {
                            Id = id,
                            DscVersion = 2,
                            Version = NuGetVersion.Parse(version),
                            Tags = tags,
                            IsLocal = false,
                        });
                    }
                }
                else
                {
                    _logger.LogError($"Failed to retrieve DSC modules from PowerShell Gallery. Status code: {response.StatusCode}");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception occurred while retrieving DSC modules from PowerShell Gallery.");
            }
        }

        return modules;
    }

    /// <inheritdoc/>
    public Task<IReadOnlySet<string>> GetDscModuleResourcesAsync(IDSCModule dscModule)
    {
        // For powershell gallery modules, we extract resource names from tags
        var dscResourcePrefix = "PSDscResource_";
        var tags = dscModule.Tags.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var resourceTags = tags.Where(tag => tag.StartsWith(dscResourcePrefix, StringComparison.Ordinal));
        var resourceNames = resourceTags.Select(r => r[dscResourcePrefix.Length..]).ToList();
        return Task.FromResult<IReadOnlySet<string>>(resourceNames.ToHashSet());
    }

    /// <summary>
    /// Constructs a URL for querying DSC (Desired State Configuration) modules
    /// from the specified base URL.
    /// </summary>
    /// <returns>A string representing the URL for retrieving DSC modules.</returns>
    private string BuildDscModuleUrl()
    {
        var builder = new UriBuilder(SearchUrl);
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["includePrerelease"] = "true";
        query["$filter"] = string.Join(" and ", [
            "IsAbsoluteLatestVersion eq true",
            "substringof('dscresource', tolower(Tags))",
        ]);
        builder.Query = query.ToString();
        return builder.ToString();
    }

    public async Task<List<DSCResourceClassDefinition>> GetDSCModuleResourcesDefinitionAsync(IDSCModule dscModule, CancellationToken ct = default)
    {
        var identity = new PackageIdentity(dscModule.Id, dscModule.Version);
        var tempPath = Path.GetTempPath();
        using var cache = new SourceCacheContext();
        var downloadContext = new PackageDownloadContext(cache);

        var download = await _repository.GetResourceAsync<DownloadResource>(ct);
        using var result = await download.GetDownloadResourceResultAsync(identity, downloadContext, tempPath, NullLogger.Instance, ct);
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
            var psm1Content = await sr.ReadToEndAsync(ct);
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

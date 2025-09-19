// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Services;

internal sealed class PowerShellGalleryModuleProvider : IModuleProvider
{
    // Base URL for PowerShell Gallery search API
    private const string BaseUrl = "https://www.powershellgallery.com/api/v2/Search()";

    private readonly ILogger<PowerShellGalleryModuleProvider> _logger;

    // Namespaces for parsing the PowerShell Gallery XML response
    private readonly XNamespace _atom;
    private readonly XNamespace _metadata;
    private readonly XNamespace _dataServices;

    private string DSCModulesUrl => BuildDscModuleUrl();

    public PowerShellGalleryModuleProvider(ILogger<PowerShellGalleryModuleProvider> logger)
    {
        _logger = logger;
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
                        modules.Add(new(this, id, version, tags));
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
    public Task<IReadOnlyList<string>> GetDscModuleResourcesAsync(IDSCModule dscModule)
    {
        var dscResourcePrefix = "PSDscResource_";
        var tags = dscModule.Tags.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var resourceTags = tags.Where(tag => tag.StartsWith(dscResourcePrefix, StringComparison.InvariantCulture));
        var resourceNames = resourceTags.Select(r => r[dscResourcePrefix.Length..]).ToList();
        return Task.FromResult<IReadOnlyList<string>>(resourceNames);
    }

    /// <summary>
    /// Constructs a URL for querying DSC (Desired State Configuration) modules
    /// from the specified base URL.
    /// </summary>
    /// <returns>A string representing the URL for retrieving DSC modules.</returns>
    private string BuildDscModuleUrl()
    {
        var builder = new UriBuilder(BaseUrl);
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["includePrerelease"] = "true";
        query["$filter"] = string.Join(" and ", [
            "IsAbsoluteLatestVersion eq true",
            "substringof('dscresource', tolower(Tags))",
        ]);
        builder.Query = query.ToString();
        return builder.ToString();
    }
}

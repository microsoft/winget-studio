// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Services;

internal sealed partial class NuGetV2Parser : INuGetV2Parser
{
    // Namespaces for parsing the PowerShell Gallery XML response
    private readonly XNamespace _atom;
    private readonly XNamespace _metadata;
    private readonly XNamespace _dataServices;

    public NuGetV2Parser()
    {
        _atom = XNamespace.Get("http://www.w3.org/2005/Atom");
        _metadata = XNamespace.Get("http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
        _dataServices = XNamespace.Get("http://schemas.microsoft.com/ado/2007/08/dataservices");
    }

    /// <inheritdoc/>
    public IReadOnlyList<ModuleMetadata> ParseFeed(string atomXml)
    {
        return [.. XDocument.Parse(atomXml)
            .Descendants(_atom + "entry")
            .Select(ParseEntryElement)
            .Where(m => m != null)];
    }

    /// <summary>
    /// Parses an individual entry element from the Atom feed.
    /// </summary>
    /// <param name="entry">The entry XElement.</param>
    /// <returns>The parsed ModuleMetadata or null if parsing fails.</returns>
    private ModuleMetadata ParseEntryElement(XElement entry)
    {
        var props = entry.Descendants(_metadata + "properties").FirstOrDefault();
        if (props != null)
        {
            var id = props.Element(_dataServices + "Id").Value;
            var version = props.Element(_dataServices + "Version").Value;
            var tags = props.Element(_dataServices + "Tags").Value;
            return new()
            {
                Id = id,
                Version = version,
                Tags = tags,
            };
        }

        return null;
    }
}

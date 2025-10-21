// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Threading.Tasks;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Services;

internal sealed partial class NuGetV2Client : INuGetV2Client
{
    private readonly HttpClient _http;
    private readonly INuGetV2Parser _parser;

    public NuGetV2Client(HttpClient http, INuGetV2Parser parser)
    {
        _http = http;
        _parser = parser;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ModuleMetadata>> SearchAsync(string baseUrl, NameValueCollection query)
    {
        var urlBuilder = new UriBuilder($"{baseUrl.TrimEnd('/')}/Search()");
        urlBuilder.Query = query.ToString();
        var response = await _http.GetAsync(urlBuilder.ToString());
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsStringAsync();
        return _parser.ParseFeed(result);
    }
}

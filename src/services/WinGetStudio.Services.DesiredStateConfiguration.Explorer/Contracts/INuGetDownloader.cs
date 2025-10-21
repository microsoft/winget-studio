// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;

public interface INuGetDownloader
{
    /// <summary>
    /// Creates a NuGet source repository using the CoreV2 protocol.
    /// </summary>
    /// <param name="sourceUrl">The URL of the NuGet source.</param>
    /// <returns>A source repository instance.</returns>
    SourceRepository CreateRepositoryCoreV2(string sourceUrl);

    /// <summary>
    /// Creates a NuGet source repository using the CoreV3 protocol.
    /// </summary>
    /// <param name="sourceUrl">The URL of the NuGet source.</param>
    /// <returns>A source repository instance.</returns>
    SourceRepository CreateRepositoryCoreV3(string sourceUrl);

    /// <summary>
    /// Opens a NuGet package from the specified repository.
    /// </summary>
    /// <param name="repository">The source repository.</param>
    /// <param name="id">The package identifier.</param>
    /// <param name="version">The package version.</param>
    /// <returns>The result of the package opening operation.</returns>
    Task<OpenPackageResult> OpenPackageAsync(SourceRepository repository, string id, NuGetVersion version);
}

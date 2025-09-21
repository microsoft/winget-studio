// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using NuGet.Versioning;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;

public interface IDSCModule
{
    bool IsLocal { get; }

    int DscVersion { get; }

    string Id { get; }

    NuGetVersion Version { get; }

    string Tags { get; }

    IReadOnlySet<string> Resources { get; }

    Task LoadDSCResourcesAsync();

    Task LoadDSCResourcesDefinitionAsync();

    DSCResource GetResourceDetails(string resourceName);
}

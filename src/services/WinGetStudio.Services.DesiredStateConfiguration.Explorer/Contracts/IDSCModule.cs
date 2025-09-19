// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using NuGet.Versioning;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;

public interface IDSCModule
{
    IModuleProvider Provider { get; }

    string Name { get; }

    NuGetVersion Version { get; }

    string Tags { get; }

    Task<IReadOnlyList<string>> GetDSCResourcesAsync();
}

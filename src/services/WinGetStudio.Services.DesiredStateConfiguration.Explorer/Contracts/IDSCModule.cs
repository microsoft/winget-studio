// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using NuGet.Versioning;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;

public interface IDSCModule
{
    IModuleProvider Provider { get; }

    string Id { get; }

    NuGetVersion Version { get; }

    string Tags { get; }

    Task LoadDSCResourcesAsync();
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using WinGetStudio.Services.Core.Extensions;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Services;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddDSCExplorer(this IServiceCollection services)
    {
        services.AddCore();
        services.AddSingleton<IModuleProvider, PowerShellGalleryModuleProvider>();
        services.AddSingleton<IDSCExplorer, DSCExplorer>();

        return services;
    }
}

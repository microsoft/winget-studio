// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WinGetStudio.Services.Core.Contracts;
using WinGetStudio.Services.Core.Services;

namespace WinGetStudio.Services.Core.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddCore(this IServiceCollection services)
    {
        services.TryAddSingleton<IMicrosoftStoreService, MicrosoftStoreService>();
        services.TryAddSingleton<IPackageDeploymentService, PackageDeploymentService>();
        return services;
    }
}

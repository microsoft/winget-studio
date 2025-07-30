// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.Services.Core.Extensions;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Services;
using Microsoft.Extensions.DependencyInjection;

namespace WinGetStudio.Services.DesiredStateConfiguration.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddDSC(this IServiceCollection services)
    {
        services.AddCore();
        services.AddSingleton<IDSC, DSC>();
        services.AddSingleton<IDSCDeployment, DSCDeployment>();
        services.AddSingleton<IDSCOperations, DSCOperations>();
        services.AddSingleton<IDSCFactory, DSCFactory>();
        services.AddSingleton<IDSCSetBuilder, DSCSetBuilder>();
        return services;
    }
}

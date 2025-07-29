// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using DSCUI.Services.Core.Extensions;
using DSCUI.Services.DesiredStateConfiguration.Contracts;
using DSCUI.Services.DesiredStateConfiguration.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DSCUI.Services.DesiredStateConfiguration.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddDSC(this IServiceCollection services)
    {
        services.AddCore();
        services.AddSingleton<IDSC, DSC>();
        services.AddSingleton<IDSCDeployment, DSCDeployment>();
        services.AddSingleton<IDSCOperations, DSCOperations>();
        return services;
    }
}

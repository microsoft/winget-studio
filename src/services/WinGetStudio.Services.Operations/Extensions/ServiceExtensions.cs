// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using WinGetStudio.Services.Core.Extensions;
using WinGetStudio.Services.Operations.Contracts;
using WinGetStudio.Services.Operations.Models;
using WinGetStudio.Services.Operations.Services;

namespace WinGetStudio.Services.Operations.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddOperations(this IServiceCollection services)
    {
        services.AddCore();
        services.AddSingleton<IOperationHub, OperationHub>();
        services.AddSingleton<IOperationExecutor, OperationExecutor>();
        services.AddSingleton<IOperationPublisher, OperationPublisher>();
        services.AddSingleton<IOperationRepository, OperationRepository>();
        services.AddSingleton<IOperationManager, OperationManager>();
        services.AddSingleton<IOperationPolicyManager, OperationPolicyManager>();

        // Factories
        services.AddTransient<OperationContextFactory>(sp => cancellationToken => ActivatorUtilities.CreateInstance<OperationContext>(sp, cancellationToken));
        services.AddTransient<OperationScopeFactory>(sp => (options, cancellationToken) => ActivatorUtilities.CreateInstance<OperationScope>(sp, options, cancellationToken));
        return services;
    }
}

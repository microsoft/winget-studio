// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using WinGetStudio.CLI.Root.Commands;
using WinGetStudio.CLI.Root.Options;

namespace WinGetStudio.CLI.Root.Extensions;

internal static class ServiceExtensions
{
    public static IServiceCollection AddRootCommandFlow(this IServiceCollection services)
    {
        // Commands
        services.AddTransient<WinGetStudioCommand>();

        // Options
        services.AddTransient<LogsOption>();

        return services;
    }
}

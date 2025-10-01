// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using WinGetStudio.CLI.Settings.Commands;

namespace WinGetStudio.CLI.Settings.Extensions;

internal static class ServiceExtensions
{
    public static IServiceCollection AddSettingsCommandFlow(this IServiceCollection services)
    {
        // Commands
        services.AddTransient<SettingsCommand>();

        return services;
    }
}

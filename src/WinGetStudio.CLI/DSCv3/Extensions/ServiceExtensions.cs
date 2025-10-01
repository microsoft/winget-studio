// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using WinGetStudio.CLI.DSCv3.Commands;
using WinGetStudio.CLI.DSCv3.Contracts;
using WinGetStudio.CLI.DSCv3.DscResources;
using WinGetStudio.CLI.DSCv3.Options;
using WinGetStudio.CLI.DSCv3.Services;

namespace WinGetStudio.CLI.DSCv3.Extensions;

internal static class ServiceExtensions
{
    public static IServiceCollection AddDscCommandFlow(this IServiceCollection services)
    {
        // Commands
        services.AddTransient<DscCommand>();
        services.AddTransient<ExportSubcommand>();
        services.AddTransient<GetSubcommand>();
        services.AddTransient<ManifestSubcommand>();
        services.AddTransient<SchemaSubcommand>();
        services.AddTransient<SetSubcommand>();
        services.AddTransient<TestSubcommand>();

        // Options
        services.AddTransient<InputOption>();
        services.AddTransient<OutputDirectoryOption>();
        services.AddTransient<ResourceOption>();

        // Services
        services.AddSingleton<IResourceProvider, ResourceProvider>();

        // Resources
        services.AddSingleton<SettingsResource>();

        return services;
    }
}

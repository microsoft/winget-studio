// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using WinGetStudio.Services.Core.Contracts;
using WinGetStudio.Services.Core.Extensions;
using WinGetStudio.Services.Core.Helpers;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Services;
using WinGetStudio.Services.DesiredStateConfiguration.Extensions;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddDSCExplorer(this IServiceCollection services)
    {
        var jsonCacheDirectory = RuntimeHelper.GetModuleCatalogCachePath();
        if (!Directory.Exists(jsonCacheDirectory))
        {
            Directory.CreateDirectory(jsonCacheDirectory);
        }

        services.AddCore();
        services.AddDSC();
        services.AddSingleton<IDSCExplorer, DSCExplorer>();
        services.AddSingleton<INuGetV2Parser, NuGetV2Parser>();
        services.AddSingleton<INuGetDownloader, NuGetDownloader>();
        services.AddSingleton<IModuleCatalogMemoryCacheProvider, ModuleCatalogMemoryCacheProvider>();
        services.AddSingleton<IModuleCatalogRepository, ModuleCatalogRepository>();
        services.AddSingleton<IDSCResourceJsonSchemaDefaultGenerator, DSCResourceJsonSchemaDefaultGenerator>();
        services.AddSingleton<IDSCProcess, DSCProcess>();
        services.AddSingleton<IModuleCatalogJsonFileCacheProvider>(sp =>
        {
            return ActivatorUtilities.CreateInstance<ModuleCatalogJsonFileCacheProvider>(sp, jsonCacheDirectory);
        });

        // HTTP clients
        services
            .AddHttpClient<INuGetV2Client, NuGetV2Client>(http =>
            {
                http.Timeout = TimeSpan.FromSeconds(30);
                http.DefaultRequestHeaders.UserAgent.ParseAdd($"WinGetStudio/{RuntimeHelper.GetAppVersion()}");
            })
            .AddStandardResilienceHandler(options =>
            {
                options.Retry.MaxRetryAttempts = 3;
                options.Retry.Delay = TimeSpan.FromMilliseconds(250);
                options.Retry.BackoffType = DelayBackoffType.Exponential;
            });

        // Module providers
        services.AddSingleton<IModuleProvider, PowerShellGalleryModuleProvider>();
        services.AddSingleton<IModuleProvider, LocalDscV3ModuleProvider>();

        // Parsers
        services.AddSingleton<IDSCResourceParser, Psm1Parser>();

        return services;
    }
}

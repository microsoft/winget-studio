// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using WinGetStudio.Services.Core.Extensions;
using WinGetStudio.Services.Core.Helpers;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Services;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddDSCExplorer(this IServiceCollection services)
    {
        services.AddCore();
        services.AddSingleton<IDSCExplorer, DSCExplorer>();
        services.AddSingleton<INuGetV2Parser, NuGetV2Parser>();
        services.AddSingleton<INuGetDownloader, NuGetDownloader>();

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

        // Parsers
        services.AddSingleton<IDSCResourceParser, Psm1Parser>();

        return services;
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using WinGetStudio.Services.Core.Extensions;
using WinGetStudio.Services.Core.Helpers;

namespace WinGetStudio.Services.Logging.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddLogging(this IServiceCollection services, string appSettingsFileName)
    {
        services.AddCore();
        services.AddSerilog(loggerConfig =>
        {
            Environment.SetEnvironmentVariable("WINGETSTUDIO_LOGS_ROOT", RuntimeHelper.GetAppInstanceLogPath());

            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(appSettingsFileName, optional: false, reloadOnChange: true)
                .Build();

            loggerConfig
                .ReadFrom.Configuration(config)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Version", RuntimeHelper.GetAppVersion());
        });

        return services;
    }
}

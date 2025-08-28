// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using WinGetStudio.Contracts.Services;

namespace WinGetStudio.Extensions;

public static class IHostBuilderExtensions
{
    /// <summary>
    /// Configure logging for the host builder.
    /// </summary>
    /// <param name="hostBuilder">The host builder.</param>
    /// <returns>The host builder with logging configured.</returns>
    public static IHostBuilder UseLogger(this IHostBuilder hostBuilder)
    {
        return hostBuilder.UseSerilog((context, services, loggerConfig) =>
        {
            var appInfo = services.GetRequiredService<IAppInfoService>();
            Environment.SetEnvironmentVariable("WINGETSTUDIO_LOGS_ROOT", appInfo.GetAppInstanceLogPath());

            // Logging
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Get assembly version from IAppInfoService
            loggerConfig
                .ReadFrom.Configuration(config)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Version", appInfo.GetAppVersion());
        });
    }
}

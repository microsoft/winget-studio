// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WinGetStudio.Services.Core.Extensions;
using WinGetStudio.Services.Core.Helpers;
using WinGetStudio.Services.Settings.Contracts;
using WinGetStudio.Services.Settings.Models;
using WinGetStudio.Services.Settings.Services;

namespace WinGetStudio.Services.Settings.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddSettings(this IServiceCollection services)
    {
        var settingsDirectory = RuntimeHelper.GetSettingsDirectory();
        if (!Directory.Exists(settingsDirectory))
        {
            Directory.CreateDirectory(settingsDirectory);
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(settingsDirectory)
            .AddJsonFile(RuntimeHelper.SettingsFile, optional: true, reloadOnChange: true)
            .Build();

        services.AddCore();
        services.Configure<GeneralSettings>(configuration);
        services.AddSingleton<IUserSettings, UserSettings>();
        return services;
    }
}

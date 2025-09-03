// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Windows.Storage;
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
        var settingsFullPath = GetSettingsFullPath(UserSettings.ApplicationDataFolder);
        if (!Directory.Exists(settingsFullPath))
        {
            Directory.CreateDirectory(settingsFullPath);
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(settingsFullPath)
            .AddJsonFile(UserSettings.SettingsFile, optional: true, reloadOnChange: true)
            .Build();

        services.AddCore();
        services.Configure<GeneralSettings>(configuration);
        services.AddSingleton<IUserSettings, UserSettings>();
        return services;
    }

    private static string GetSettingsFullPath(string settingsPath)
    {
        return RuntimeHelper.IsMSIX
             ? ApplicationData.Current.LocalFolder.Path
             : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), settingsPath);
    }
}

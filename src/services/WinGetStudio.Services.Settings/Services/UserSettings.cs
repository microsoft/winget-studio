// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Windows.Storage;
using WinGetStudio.Services.Core.Contracts;
using WinGetStudio.Services.Core.Helpers;
using WinGetStudio.Services.Settings.Contracts;
using WinGetStudio.Services.Settings.Models;

namespace WinGetStudio.Services.Settings.Services;

internal sealed class UserSettings : IUserSettings
{
    private readonly IOptionsMonitor<IGeneralSettings> _settingsOptions;
    private readonly IFileService _fileService;
    private readonly object _lock = new();
    private readonly ILogger<UserSettings> _logger;

    public const string ApplicationDataFolder = "WinGetStudio/ApplicationData";
    public const string SettingsFile = "settings.json";

    public string FullPath => Path.Combine(GetSettingsDirectory(), SettingsFile);

    public IGeneralSettings Current => _settingsOptions.CurrentValue;

    public event EventHandler<IGeneralSettings> SettingsChanged;

    public UserSettings(
        IOptionsMonitor<GeneralSettings> settingsOptions,
        IFileService fileService,
        ILogger<UserSettings> logger)
    {
        _settingsOptions = settingsOptions;
        _fileService = fileService;
        _logger = logger;

        _settingsOptions.OnChange(OnSettingsChanged);

        EnsureFileExists();
    }

    public void Save(Action<GeneralSettings> changes = null)
    {
        lock (_lock)
        {
            var settings = Current.Clone();
            changes?.Invoke(settings);
            if (_fileService.TrySaveJson(FullPath, settings))
            {
                _logger.LogInformation("Settings saved successfully.");
            }
            else
            {
                _logger.LogWarning("Failed to save settings. Falling back to default settings.");
            }
        }
    }

    public static string GetSettingsDirectory()
    {
        return RuntimeHelper.IsMSIX
             ? ApplicationData.Current.LocalFolder.Path
             : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ApplicationDataFolder);
    }

    private void EnsureFileExists()
    {
        _logger.LogInformation($"Ensuring settings file exists at: {FullPath}");
        if (!File.Exists(FullPath))
        {
            _logger.LogInformation("Settings file does not exist. Creating default settings file.");
            Save();
        }
        else
        {
            _logger.LogInformation("Settings file already exists.");
        }
    }

    private void OnSettingsChanged(IGeneralSettings settings)
    {
        SettingsChanged?.Invoke(this, settings);
        _logger.LogInformation("Settings have changed and event has been raised.");
    }
}

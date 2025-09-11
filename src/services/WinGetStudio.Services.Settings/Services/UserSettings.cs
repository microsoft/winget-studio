// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Windows.Storage;
using WinGetStudio.Services.Core.Contracts;
using WinGetStudio.Services.Core.Helpers;
using WinGetStudio.Services.Settings.Contracts;
using WinGetStudio.Services.Settings.Models;

namespace WinGetStudio.Services.Settings.Services;

internal sealed partial class UserSettings : IUserSettings, IDisposable
{
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly IOptionsMonitor<GeneralSettings> _settingsOptions;
    private readonly IFileService _fileService;
    private readonly ILogger<UserSettings> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private bool _disposedValue;

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
        _serializerOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true,
        };

        _settingsOptions.OnChange(OnSettingsChanged);
    }

    /// <inheritdoc/>
    public async Task InitializeAsync()
    {
        await EnsureFileExistsAsync();
    }

    /// <inheritdoc/>
    public async Task SaveAsync(Action<GeneralSettings> changes)
    {
        ArgumentNullException.ThrowIfNull(changes);
        var newSettings = Current.Clone();
        changes(newSettings);

        if (_settingsOptions.CurrentValue.Equals(newSettings))
        {
            _logger.LogInformation("No changes detected in settings. Save operation skipped.");
        }
        else
        {
            await SaveInternalAsync(newSettings);
        }
    }

    public static string GetSettingsDirectory()
    {
        return RuntimeHelper.IsMSIX
             ? ApplicationData.Current.LocalFolder.Path
             : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ApplicationDataFolder);
    }

    /// <summary>
    /// Ensures that the settings file exists. If it does not exist, creates a
    /// new file with default settings.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task EnsureFileExistsAsync()
    {
        _logger.LogInformation($"Ensuring settings file exists at: {FullPath}");
        if (!File.Exists(FullPath))
        {
            _logger.LogInformation("Settings file does not exist. Creating default settings file.");
            await SaveInternalAsync(_settingsOptions.CurrentValue);
        }
        else
        {
            _logger.LogInformation("Settings file already exists.");
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _lock.Dispose();
            }

            _disposedValue = true;
        }
    }

    /// <summary>
    /// Saves the provided settings to the settings file in a thread-safe
    /// manner.
    /// </summary>
    /// <param name="newSettings">The new settings to save.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    private async Task SaveInternalAsync(GeneralSettings newSettings)
    {
        await _lock.WaitAsync();
        try
        {
            if (await _fileService.TrySaveJsonAsync(FullPath, newSettings, _serializerOptions))
            {
                _logger.LogInformation("Settings saved successfully.");
            }
            else
            {
                _logger.LogWarning("Failed to save settings. Falling back to default settings.");
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    private void OnSettingsChanged(IGeneralSettings settings)
    {
        SettingsChanged?.Invoke(this, settings);
        _logger.LogInformation("Settings have changed and event has been raised.");
    }
}

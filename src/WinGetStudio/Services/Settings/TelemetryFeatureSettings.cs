// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.Contracts.Services;
using WinGetStudio.Services.Settings.Contracts;
using WinGetStudio.Services.Settings.Models;
using WinGetStudio.Services.Telemetry.Contracts;

namespace WinGetStudio.Services.Settings;

internal sealed class TelemetryFeatureSettings : IFeatureSettingsService
{
    private readonly IUserSettings _userSettings;
    private readonly ITelemetryService _telemetry;

    public bool IsDisabled => _userSettings.Current.Telemetry.Disable;

    public TelemetryFeatureSettings(IUserSettings userSettings, ITelemetryService telemetry)
    {
        _userSettings = userSettings;
        _telemetry = telemetry;

        _userSettings.SettingsChanged += OnSettingsChanged;
    }

    /// <inheritdoc/>
    public async Task ApplySettingsAsync()
    {
        _telemetry.Configure(IsDisabled);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Handles the SettingsChanged event of the UserSettings service.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="newSettings">The new settings.</param>
    private void OnSettingsChanged(object? sender, GeneralSettings newSettings)
    {
        _telemetry.Configure(newSettings.Telemetry.Disable);
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.Contracts.Services;
using WinGetStudio.Services.Settings.Contracts;
using WinGetStudio.Services.Settings.Models;
using WinGetStudio.Services.Telemetry.Contracts;

namespace WinGetStudio.Services;

internal sealed class TelemetrySettingsService : ITelemetrySettingsService
{
    private readonly IUserSettings _userSettings;
    private readonly ITelemetryService _telemetry;

    public bool IsDisabled => _userSettings.Current.Telemetry.Disable;

    public TelemetrySettingsService(IUserSettings userSettings, ITelemetryService telemetry)
    {
        _userSettings = userSettings;
        _telemetry = telemetry;

        _userSettings.SettingsChanged += OnSettingsChanged;
    }

    public void ApplySettings()
    {
        _telemetry.Configure(IsDisabled);
    }

    private void OnSettingsChanged(object? sender, GeneralSettings newSettings)
    {
        _telemetry.Configure(newSettings.Telemetry.Disable);
    }
}

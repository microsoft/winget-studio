// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WinGetStudio.Contracts.Services;

public interface ITelemetrySettingsService
{
    /// <summary>
    /// Gets a value indicating whether telemetry is disabled.
    /// </summary>
    bool IsDisabled { get; }

    /// <summary>
    /// Applies the telemetry settings.
    /// </summary>
    void ApplySettings();
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.Services.Telemetry.Models;

namespace WinGetStudio.Services.Telemetry.Contracts;

public interface ITelemetry
{
    /// <summary>
    /// Publishes ETW event when an action is triggered on.
    /// </summary>
    /// <typeparam name="T">Telemetry event type.</typeparam>
    /// <param name="telemetryEvent">Telemetry event data object.</param>
    void WriteEvent<T>(T telemetryEvent)
        where T : EventBase;

    /// <summary>
    /// Disable all telemetry events.
    /// </summary>
    public void DisableEvents();
}

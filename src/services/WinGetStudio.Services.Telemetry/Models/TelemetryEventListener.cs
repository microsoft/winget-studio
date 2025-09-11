// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.Tracing;

namespace WinGetStudio.Services.Telemetry.Models;

/// <summary>
/// Telemetry Event Listener class mainly used to disable telemetry events.
/// </summary>
internal sealed partial class TelemetryEventListener : EventListener
{
    private readonly EventSource _eventSource;

    public TelemetryEventListener(EventSource eventSource)
    {
        _eventSource = eventSource;
    }

    /// <summary>
    /// Disables all telemetry events
    /// </summary>
    public void DisableEvents() => DisableEvents(_eventSource);
}

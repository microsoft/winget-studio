// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.Tracing;
using Microsoft.Diagnostics.Telemetry;
using WinGetStudio.Services.Telemetry.Contracts;
using WinGetStudio.Services.Telemetry.Models;

namespace WinGetStudio.Services.Telemetry.Services;

/// <summary>
/// Telemetry service implementation.
/// </summary>
internal sealed partial class Telemetry : TelemetryEventSource, ITelemetry
{
    private readonly TelemetryEventListener _telemetryEventListener;
    private const string EventSourceName = "Microsoft.WinGetStudio";

    /// <summary>
    /// Initializes a new instance of the <see cref="Telemetry"/> class.
    /// </summary>
    public Telemetry()
        : base(EventSourceName, TelemetryGroup.MicrosoftTelemetry)
    {
        _telemetryEventListener = new(this);
    }

    /// <inheritdoc/>
    public void WriteEvent<T>(T telemetryEvent)
        where T : EventBase
    {
        Write<T>(
            null,
            new EventSourceOptions()
            {
                Keywords = CriticalDataKeyword,
            },
            telemetryEvent);

        EventBase.IncrementCorrelationVector();
    }

    /// <inheritdoc/>
    public void DisableEvents()
    {
        _telemetryEventListener.DisableEvents();
    }
}

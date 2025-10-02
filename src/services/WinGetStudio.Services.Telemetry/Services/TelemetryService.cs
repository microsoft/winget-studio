// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.Tracing;
using Microsoft.Diagnostics.Telemetry;
using Microsoft.Extensions.Logging;
using WinGetStudio.Services.Telemetry.Contracts;
using WinGetStudio.Services.Telemetry.Models;

namespace WinGetStudio.Services.Telemetry.Services;

/// <summary>
/// Telemetry service implementation.
/// </summary>
internal sealed partial class TelemetryService : TelemetryEventSource, ITelemetryService
{
    private const string EventSourceName = "Microsoft.WinGetStudio";
    private readonly object _lock = new();
    private readonly ILogger<TelemetryService> _logger;

    private bool _isConfigured;
    private bool _isDisabled;

    /// <summary>
    /// Initializes a new instance of the <see cref="TelemetryService"/> class.
    /// </summary>
    public TelemetryService(ILogger<TelemetryService> logger)
        : base(EventSourceName, TelemetryGroup.MicrosoftTelemetry)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public void WriteEvent<T>(T telemetryEvent)
        where T : EventBase
    {
        lock (_lock)
        {
            if (!_isConfigured)
            {
                _logger.LogWarning("Telemetry cannot be written before the service is configured.");
            }
            else if (!_isDisabled)
            {
                Write<T>(null, new EventSourceOptions() { Keywords = CriticalDataKeyword }, telemetryEvent);
                EventBase.IncrementCorrelationVector();
            }
        }
    }

    public void Configure(bool disableEvents)
    {
        lock (_lock)
        {
            _isDisabled = disableEvents;
            _isConfigured = true;
        }
    }
}

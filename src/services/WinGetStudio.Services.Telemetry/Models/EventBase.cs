// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.Tracing;
using System.Reflection;
using Microsoft.CorrelationVector;
using Microsoft.Diagnostics.Telemetry.Internal;

namespace WinGetStudio.Services.Telemetry.Models;

/// <summary>
/// A base class to implement properties that are common to all telemetry events.
/// </summary>
[EventData]
public abstract class EventBase
{
    private static readonly CorrelationVector _correlationVector = new(CorrelationVectorVersion.V2);

    /// <summary>
    /// Gets a value indicating whether to replace the UTC app session GUID.
    /// </summary>
    public bool UTCReplace_AppSessionGuid => true;

    /// <summary>
    /// Gets the app version from the assembly.
    /// </summary>
    public string AppVersion { get; } = Assembly.GetEntryAssembly().GetName().Version.ToString();

    /// <summary>
    /// Gets the correlation vector value associated with the event.
    /// </summary>
    public string __TlgCV__ => _correlationVector.Value;

    /// <summary>
    /// Gets or sets a value indicating whether the event was successful.
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// Gets the privacy datatype tag for the telemetry event.
    /// </summary>
    public abstract PartA_PrivTags PartA_PrivTags { get; }

    /// <summary>
    /// Increments the correlation vector extension value.
    /// </summary>
    internal static void IncrementCorrelationVector() => _correlationVector.Increment();
}

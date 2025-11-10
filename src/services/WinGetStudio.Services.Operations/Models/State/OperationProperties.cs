// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace WinGetStudio.Services.Operations.Models.State;

public sealed record class OperationProperties(
    string? Title,
    string? Message,
    int? Percent,
    OperationStatus Status,
    OperationSeverity Severity,
    IReadOnlyList<OperationAction> Actions)
{
    /// <summary>
    /// Gets an empty operation properties.
    /// </summary>
    public static OperationProperties Empty { get; } = new(
        null,
        null,
        null,
        OperationStatus.NotStarted,
        OperationSeverity.Info,
        []);
}

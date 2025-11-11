// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace WinGetStudio.Services.Operations.Models.State;

/// <summary>
/// Represents the properties of an operation.
/// </summary>
/// <param name="Title">The title of the operation.</param>
/// <param name="Message">The message of the operation.</param>
/// <param name="Percent">The percent complete of the operation or null if the percent is indeterminate.</param>
/// <param name="Status">The status of the operation.</param>
/// <param name="Severity">The severity of the operation.</param>
/// <param name="Actions">The actions associated with the operation.</param>
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

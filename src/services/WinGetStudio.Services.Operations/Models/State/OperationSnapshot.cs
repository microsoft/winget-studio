// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace WinGetStudio.Services.Operations.Models.State;

/// <summary>
/// Represents a snapshot of an operation at a specific point in time.
/// </summary>
/// <param name="Id">The unique identifier of the operation.</param>
/// <param name="CreatedAt">The timestamp when the operation was created.</param>
/// <param name="UpdatedAt">The timestamp when the operation was last updated.</param>
/// <param name="Properties">The properties of the operation.</param>
public sealed record class OperationSnapshot(
    Guid Id,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    OperationProperties Properties)
{
    /// <summary>
    /// Gets an empty operation snapshot.
    /// </summary>
    public static OperationSnapshot Empty { get; } = new(
        Guid.Empty,
        DateTimeOffset.MinValue,
        DateTimeOffset.MinValue,
        OperationProperties.Empty);
}

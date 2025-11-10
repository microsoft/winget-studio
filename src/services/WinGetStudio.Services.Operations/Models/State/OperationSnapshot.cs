// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace WinGetStudio.Services.Operations.Models.State;

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

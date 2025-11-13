// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace WinGetStudio.Services.Operations.Models.States;

/// <summary>
/// Represents an operation notification.
/// </summary>
/// <param name="Id">The source operation ID.</param>
/// <param name="Duration">The notification duration.</param>
/// <param name="Properties">The operation properties.</param>
public sealed record class OperationNotification(Guid Id, TimeSpan Duration, OperationProperties Properties)
{
    public static TimeSpan DefaultDuration { get; } = TimeSpan.FromSeconds(3);
}

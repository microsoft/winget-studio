// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace WinGetStudio.Services.Operations.Models.State;

/// <summary>
/// Represents an operation notification.
/// </summary>
/// <param name="Id">The source operation ID.</param>
/// <param name="Properties">The operation properties.</param>
public sealed record class OperationNotification(Guid Id, OperationProperties Properties);

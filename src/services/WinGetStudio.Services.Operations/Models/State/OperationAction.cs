// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;

namespace WinGetStudio.Services.Operations.Models.State;

/// <summary>
/// Represents an action that can be taken for an operation.
/// </summary>
/// <param name="Id">The unique identifier for the action.</param>
/// <param name="Text">The text to display for the action.</param>
/// <param name="IsPrimary">Whether the action is a primary action.</param>
/// <param name="Action">The action to perform.</param>
public sealed record class OperationAction(string Text, bool IsPrimary, Func<Task> Action)
{
    public Guid Id { get; } = Guid.NewGuid();
}

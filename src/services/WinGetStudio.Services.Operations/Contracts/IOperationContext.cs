// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading;
using WinGetStudio.Services.Operations.Models.State;

namespace WinGetStudio.Services.Operations.Contracts;

/// <summary>
/// Represents the context of an operation.
/// </summary>
public interface IOperationContext
{
    /// <summary>
    /// Gets the unique identifier of the operation.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Gets the cancellation token for the operation.
    /// </summary>
    CancellationToken CancellationToken { get; }

    /// <summary>
    /// Gets the current snapshot of the operation.
    /// </summary>
    OperationSnapshot CurrentSnapshot { get; }

    /// <summary>
    /// Updates the operation properties by applying the provided mutation function.
    /// </summary>
    /// <param name="mutate">The mutation function to apply.</param>
    void Update(Func<OperationProperties, OperationProperties> mutate);

    /// <summary>
    /// Notifies observers of the current operation state, optionally applying a mutation function.
    /// </summary>
    /// <param name="mutate">The optional mutation function to apply.</param>
    void Notify(Func<OperationProperties, OperationProperties>? mutate = null);

    /// <summary>
    /// Cancels the operation.
    /// </summary>
    void Cancel();
}

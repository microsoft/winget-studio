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
    /// Commits a mutation to the current snapshot of the operation and
    /// publishes to the snapshot stream.
    /// </summary>
    /// <param name="mutate">The mutation function to apply.</param>
    void CommitSnapshot(Func<OperationProperties, OperationProperties> mutate);

    /// <summary>
    /// Publishes the current snapshot of the operation with an optional mutation.
    /// </summary>
    /// <remarks>The mutation is not committed to the operation's state.</remarks>
    /// <param name="mutate">The optional mutation function to apply.</param>
    /// <param name="duration">The optional duration for the notification.</param>
    void PublishNotification(Func<OperationProperties, OperationProperties>? mutate = null, TimeSpan? duration = null);

    /// <summary>
    /// Cancels the operation.
    /// </summary>
    void Cancel();

    /// <inheritdoc cref="IOperationManager.Publish(Models.OperationContext)" />
    void Publish();

    /// <inheritdoc cref="IOperationManager.Unpublish(Models.OperationContext)" />
    void Unpublish();
}

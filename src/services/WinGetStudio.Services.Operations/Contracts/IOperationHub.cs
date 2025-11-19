// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WinGetStudio.Services.Operations.Models;
using WinGetStudio.Services.Operations.Models.States;

namespace WinGetStudio.Services.Operations.Contracts;

public interface IOperationHub
{
    /// <inheritdoc cref="IOperationPublisher.Snapshots"/>
    IEventStream<IReadOnlyList<OperationSnapshot>> Snapshots { get; }

    /// <inheritdoc cref="IOperationPublisher.Notifications"/>
    IEventStream<OperationNotification> Notifications { get; }

    /// <inheritdoc cref="IOperationPublisher.GlobalActivity"/>
    IEventStream<GlobalActivity> GlobalActivity { get; }

    /// <inheritdoc cref="IOperationExecutor.ExecuteAsync{T}(IOperation{T}, OperationExecutionOptions?, CancellationToken)"/>
    Task<T> ExecuteAsync<T>(IOperation<T> operation, OperationExecutionOptions? options = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="IOperationExecutor.ExecuteAsync(Func{IOperationContext, Task}, OperationExecutionOptions?, CancellationToken)"/>
    Task ExecuteAsync(Func<IOperationContext, Task> operation, OperationExecutionOptions? options = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="IOperationExecutor.ExecuteAsync{T}(Func{IOperationContext, Task{T}}, OperationExecutionOptions?, CancellationToken)"/>
    Task<T> ExecuteAsync<T>(Func<IOperationContext, Task<T>> operation, OperationExecutionOptions? options = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="IOperationExecutor.BeginOperationAsync(OperationExecutionOptions?, CancellationToken)"/>
    Task<IOperationScope> BeginOperationAsync(OperationExecutionOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts broadcasting snapshots to subscribers.
    /// </summary>
    /// <param name="snapshot">The operation snapshot.</param>
    void StartSnapshotBroadcast(OperationSnapshot snapshot);

    /// <summary>
    /// Stops broadcasting snapshots to subscribers.
    /// </summary>
    /// <param name="id">The operation ID.</param>
    void StopSnapshotBroadcast(Guid id);
}

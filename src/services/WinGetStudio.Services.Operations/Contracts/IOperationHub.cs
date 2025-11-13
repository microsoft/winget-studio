// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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

    /// <inheritdoc cref="IOperationExecutor.ExecuteAsync(IOperation, CancellationToken)"/>
    Task ExecuteAsync(IOperation operation, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="IOperationExecutor.ExecuteAsync(Func{IOperationContext, Task}, OperationExecutionOptions?, CancellationToken)"/>
    Task ExecuteAsync(Func<IOperationContext, Task> operation, OperationExecutionOptions? options = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="IOperationExecutor.ExecuteAsync{T}(Func{IOperationContext, Task{T}}, OperationExecutionOptions?, CancellationToken)"/>
    Task<T> ExecuteAsync<T>(Func<IOperationContext, Task<T>> operation, OperationExecutionOptions? options = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="IOperationExecutor.BeginOperationAsync(OperationExecutionOptions?, CancellationToken)"/>
    Task<IOperationScope> BeginOperationAsync(OperationExecutionOptions? options = null, CancellationToken cancellationToken = default);
}

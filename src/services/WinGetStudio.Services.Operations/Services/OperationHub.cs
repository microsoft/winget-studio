// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WinGetStudio.Services.Operations.Contracts;
using WinGetStudio.Services.Operations.Models;
using WinGetStudio.Services.Operations.Models.States;

namespace WinGetStudio.Services.Operations.Services;

internal sealed partial class OperationHub : IOperationHub
{
    private readonly IOperationPublisher _publisher;
    private readonly IOperationExecutor _executor;
    private readonly IOperationManager _manager;

    /// <inheritdoc/>
    public IEventStream<IReadOnlyList<OperationSnapshot>> Snapshots => _publisher.Snapshots;

    /// <inheritdoc/>
    public IEventStream<OperationNotification> Notifications => _publisher.Notifications;

    /// <inheritdoc/>
    public IEventStream<GlobalActivity> GlobalActivity => _publisher.GlobalActivity;

    public OperationHub(IOperationPublisher publisher, IOperationExecutor executor, IOperationManager manager)
    {
        _publisher = publisher;
        _executor = executor;
        _manager = manager;
    }

    /// <inheritdoc/>
    public Task<T> ExecuteAsync<T>(IOperation<T> operation, CancellationToken cancellationToken = default) => _executor.ExecuteAsync(operation, cancellationToken);

    /// <inheritdoc/>
    public Task ExecuteAsync(Func<IOperationContext, Task> operation, OperationExecutionOptions? options = null, CancellationToken cancellationToken = default) => _executor.ExecuteAsync(operation, options, cancellationToken);

    /// <inheritdoc/>
    public Task<T> ExecuteAsync<T>(Func<IOperationContext, Task<T>> operation, OperationExecutionOptions? options = null, CancellationToken cancellationToken = default) => _executor.ExecuteAsync(operation, options, cancellationToken);

    /// <inheritdoc/>
    public Task<IOperationScope> BeginOperationAsync(OperationExecutionOptions? options = null, CancellationToken cancellationToken = default) => _executor.BeginOperationAsync(options, cancellationToken);

    /// <inheritdoc/>
    public void StopSnapshotBroadcast(Guid id) => _manager.RemoveOperationSnapshot(id);
}

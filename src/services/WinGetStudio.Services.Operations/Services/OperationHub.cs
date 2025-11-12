// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WinGetStudio.Services.Operations.Contracts;
using WinGetStudio.Services.Operations.Models.State;

namespace WinGetStudio.Services.Operations.Services;

internal sealed partial class OperationHub : IOperationHub
{
    private readonly IOperationPublisher _publisher;
    private readonly IOperationExecutor _executor;

    /// <inheritdoc/>
    public IEventStream<IReadOnlyList<OperationSnapshot>> Snapshots => _publisher.Snapshots;

    /// <inheritdoc/>
    public IEventStream<OperationNotification> Notifications => _publisher.Notifications;

    /// <inheritdoc/>
    public IEventStream<GlobalActivity> GlobalActivity => _publisher.GlobalActivity;

    public OperationHub(IOperationPublisher publisher, IOperationExecutor executor)
    {
        _publisher = publisher;
        _executor = executor;
    }

    /// <inheritdoc/>
    public Task ExecuteAsync(IOperation operation) => _executor.ExecuteAsync(operation);

    /// <inheritdoc/>
    public Task ExecuteAsync(Func<IOperationContext, Task> operation) => _executor.ExecuteAsync(operation);

    /// <inheritdoc/>
    public Task<T> ExecuteAsync<T>(Func<IOperationContext, Task<T>> operation) => _executor.ExecuteAsync(operation);
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.Models.Operations;
using WinGetStudio.Services.Operations.Contracts;
using WinGetStudio.Services.Operations.Models;
using WinGetStudio.Services.Operations.Models.States;

namespace WinGetStudio.Contracts.Services;

public interface IAppOperationHub
{
    /// <inheritdoc cref="IOperationHub.Snapshots"/>
    IEventStream<IReadOnlyList<OperationSnapshot>> Snapshots { get; }

    /// <inheritdoc cref="IOperationHub.Notifications"/>
    IEventStream<OperationNotification> Notifications { get; }

    /// <inheritdoc cref="IOperationHub.GlobalActivity"/>
    IEventStream<GlobalActivity> GlobalActivity { get; }

    /// <summary>
    /// Execute an operation.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="options">The execution options.</param>
    /// <param name="operation">The operation.</param>
    /// <returns>The operation result.</returns>
    Task<OperationResult<TResult>> ExecuteAsync<TResult>(OperationExecutionOptions options, Func<IOperationContext, IOperationFactory, Task<OperationResult<TResult>>> operation);

    /// <summary>
    /// Execute an operation.
    /// </summary>
    /// <param name="options">The execution options.</param>
    /// <param name="operation">The operation.</param>
    Task ExecuteAsync(OperationExecutionOptions options, Func<IOperationContext, IOperationFactory, Task> operation);

    /// <summary>
    /// Execute an operation.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="options">The execution options.</param>
    /// <param name="operation">The operation.</param>
    /// <returns>The operation result.</returns>
    Task<OperationResult<TResult>> ExecuteAsync<TResult>(OperationExecutionOptions options, IOperation<OperationResult<TResult>> operation);

    /// <inheritdoc cref="IOperationHub.BeginOperationAsync(OperationExecutionOptions?, CancellationToken)"/>
    Task<IOperationScope> BeginOperationAsync(OperationExecutionOptions? options = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="IOperationHub.GetOperationSnapshot(Guid)"/>/>
    void StopSnapshotBroadcast(Guid id);
}

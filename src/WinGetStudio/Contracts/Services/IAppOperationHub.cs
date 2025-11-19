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
    /// Run an operation with the given operation function.
    /// </summary>
    /// <param name="operation">The operation function.</param>
    Task RunAsync(Func<IOperationContext, IOperationFactory, Task> operation);

    /// <summary>
    /// Run an operation with progress with the given properties mutation and operation function.
    /// </summary>
    /// <param name="mutate">The properties mutation function.</param>
    /// <param name="operation">The operation function.</param>
    /// <param name="canCancel">A value indicating whether the operation can be canceled.</param><
    Task RunWithProgressAsync(Func<OperationProperties, OperationProperties> mutate, Func<IOperationContext, IOperationFactory, Task> operation, bool canCancel = true);

    /// <summary>
    /// Run an operation with progress with the given properties mutation and operation function.
    /// </summary>
    /// <param name="mutate">The properties mutation function.</param>
    /// <param name="operation">The operation function.</param>
    /// <param name="canCancel">A value indicating whether the operation can be canceled.</param>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <returns>The operation result.</returns>
    Task<OperationResult<TResult>> RunWithProgressAsync<TResult>(Func<OperationProperties, OperationProperties> mutate, Func<IOperationContext, IOperationFactory, Task<OperationResult<TResult>>> operation, bool canCancel = true);

    /// <inheritdoc cref="IOperationHub.BeginOperationAsync(OperationExecutionOptions?, CancellationToken)"/>
    Task<IOperationScope> BeginOperationAsync(OperationExecutionOptions? options = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="IOperationHub.GetOperationSnapshot(Guid)"/>/>
    void StopSnapshotBroadcast(Guid id);
}

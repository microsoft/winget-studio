// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using WinGetStudio.Services.Operations.Models.States;

namespace WinGetStudio.Services.Operations.Contracts;

/// <summary>
/// Represents an executor for operations.
/// </summary>
internal interface IOperationExecutor
{
    /// <summary>
    /// Executes the specified operation.
    /// </summary>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task ExecuteAsync(IOperation operation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the specified operation function.
    /// </summary>
    /// <param name="operation">The operation function to execute.</param>
    /// <param name="options">The operation execution options.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task ExecuteAsync(Func<IOperationContext, Task> operation, OperationExecutionOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the specified operation function and returns a result.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="operation">The operation function to execute.</param>
    /// <param name="options">The operation execution options.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    Task<T> ExecuteAsync<T>(Func<IOperationContext, Task<T>> operation, OperationExecutionOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new operation scope.
    /// </summary>
    /// <param name="options">The operation execution options.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The operation scope.</returns>
    Task<IOperationScope> BeginOperationAsync(OperationExecutionOptions? options = null, CancellationToken cancellationToken = default);
}

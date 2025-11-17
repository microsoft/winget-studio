// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.Models.Operations;
using WinGetStudio.Services.Operations.Contracts;
using WinGetStudio.Services.Operations.Models;

namespace WinGetStudio.Contracts.Services;

public interface IAppOperationHub
{
    /// <summary>
    /// Execute an operation.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="options">The execution options.</param>
    /// <param name="operation">The operation.</param>
    /// <returns>The operation result.</returns>
    Task<OperationResult<TResult>> ExecuteAsync<TResult>(OperationExecutionOptions options, Func<IOperationContext, Task<OperationResult<TResult>>> operation);

    /// <summary>
    /// Execute an operation.
    /// </summary>
    /// <param name="options">The execution options.</param>
    /// <param name="operation">The operation.</param>
    Task ExecuteAsync(OperationExecutionOptions options, Func<IOperationContext, Task> operation);

    /// <summary>
    /// Execute an operation.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="options">The execution options.</param>
    /// <param name="operation">The operation.</param>
    /// <returns>The operation result.</returns>
    Task<OperationResult<TResult>> ExecuteAsync<TResult>(OperationExecutionOptions options, IOperation<OperationResult<TResult>> operation);
}

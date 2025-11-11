// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;

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
    Task ExecuteAsync(IOperation operation);

    /// <summary>
    /// Executes the specified operation function.
    /// </summary>
    /// <param name="operation">The operation function to execute.</param>
    Task ExecuteAsync(Func<IOperationContext, Task> operation);
}

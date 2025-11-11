// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;

namespace WinGetStudio.Services.Operations.Contracts;

/// <summary>
/// Represents an operation that can be executed.
/// </summary>
public interface IOperation
{
    /// <summary>
    /// Executes the operation asynchronously.
    /// </summary>
    /// <param name="context">The operation context.</param>
    Task ExecuteAsync(IOperationContext context);
}

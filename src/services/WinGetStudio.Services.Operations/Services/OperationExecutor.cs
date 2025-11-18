// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using WinGetStudio.Services.Operations.Contracts;
using WinGetStudio.Services.Operations.Models;

namespace WinGetStudio.Services.Operations.Services;

internal sealed class OperationExecutor : IOperationExecutor
{
    private readonly OperationScopeFactory _scopeFactory;

    public OperationExecutor(OperationScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    /// <inheritdoc/>
    public async Task<T> ExecuteAsync<T>(IOperation<T> operation, OperationExecutionOptions? options = null, CancellationToken cancellationToken = default)
    {
         return await ExecuteAsync(operation.ExecuteAsync, options, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task ExecuteAsync(
        Func<IOperationContext, Task> operation,
        OperationExecutionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        await ExecuteAsync<Task>(
            async context =>
            {
                await operation(context);
                return Task.CompletedTask;
            },
            options,
            cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<T> ExecuteAsync<T>(
        Func<IOperationContext, Task<T>> operation,
        OperationExecutionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        await using var scope = await BeginOperationAsync(options, cancellationToken);
        return await operation(scope.Context);
    }

    /// <inheritdoc/>
    public async Task<IOperationScope> BeginOperationAsync(
        OperationExecutionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new OperationExecutionOptions();
        var scope = _scopeFactory(options, cancellationToken);
        await scope.BeginAsync();
        return scope;
    }
}

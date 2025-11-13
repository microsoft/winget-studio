// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using WinGetStudio.Services.Operations.Contracts;
using WinGetStudio.Services.Operations.Models;
using WinGetStudio.Services.Operations.Models.States;

namespace WinGetStudio.Services.Operations.Services;

internal sealed class OperationExecutor : IOperationExecutor
{
    private readonly OperationScopeFactory _scopeFactory;

    public OperationExecutor(OperationScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    /// <inheritdoc/>
    public async Task ExecuteAsync(IOperation operation, CancellationToken cancellationToken = default)
    {
         await ExecuteAsync(operation.ExecuteAsync, operation.Options, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task ExecuteAsync(
        Func<IOperationContext, Task> operation,
        OperationExecutionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        _ = await ExecuteAsync(
            async ctx =>
            {
                await operation(ctx);
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
        options ??= OperationExecutionOptions.Default;
        var scope = _scopeFactory(options, cancellationToken);
        await scope.BeginAsync();
        return scope;
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WinGetStudio.Services.Operations.Contracts;
using WinGetStudio.Services.Operations.Models;
using WinGetStudio.Services.Operations.Models.States;

namespace WinGetStudio.Services.Operations.Services;

internal sealed class OperationExecutor : IOperationExecutor
{
    private readonly ILogger<OperationExecutor> _logger;
    private readonly IOperationPolicyManager _policyManager;
    private readonly OperationContextFactory _contextFactory;

    public OperationExecutor(
        ILogger<OperationExecutor> logger,
        IOperationPolicyManager policyManager,
        OperationContextFactory contextFactory)
    {
        _logger = logger;
        _policyManager = policyManager;
        _contextFactory = contextFactory;
    }

    /// <inheritdoc/>
    public async Task ExecuteAsync(IOperation operation) => await ExecuteAsync(operation.ExecuteAsync, operation.Options);

    /// <inheritdoc/>
    public async Task ExecuteAsync(Func<IOperationContext, Task> operation, OperationExecutionOptions? options = null)
    {
        _ = await ExecuteAsync(async ctx =>
        {
            await operation(ctx);
            return Task.CompletedTask;
        });
    }

    /// <inheritdoc/>
    public async Task<T> ExecuteAsync<T>(Func<IOperationContext, Task<T>> operation, OperationExecutionOptions? options = null)
    {
        using var ctx = _contextFactory();
        try
        {
            await _policyManager.ApplyPoliciesAsync<IOperationStartPolicy>(options?.Policies, ctx);
            _logger.LogInformation($"Starting operation {ctx.Id}");
            var result = await operation(ctx);
            _logger.LogInformation($"Operation {ctx.Id} completed");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Operation {ctx.Id} failed");
            throw;
        }
        finally
        {
            await _policyManager.ApplyPoliciesAsync<IOperationCompletionPolicy>(options?.Policies, ctx);
            _logger.LogInformation($"Disposing operation {ctx.Id}");
        }
    }
}

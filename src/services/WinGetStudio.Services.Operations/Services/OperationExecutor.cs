// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WinGetStudio.Services.Operations.Contracts;
using WinGetStudio.Services.Operations.Models;

namespace WinGetStudio.Services.Operations.Services;

internal sealed class OperationExecutor : IOperationExecutor
{
    private readonly ILogger<OperationExecutor> _logger;
    private readonly IOperationRepository _repository;
    private readonly OperationContextFactory _contextFactory;

    public OperationExecutor(
        ILogger<OperationExecutor> logger,
        IOperationRepository repository,
        OperationContextFactory contextFactory)
    {
        _repository = repository;
        _logger = logger;
        _contextFactory = contextFactory;
    }

    /// <inheritdoc/>
    public async Task ExecuteAsync(IOperation operation) => await ExecuteAsync(operation.ExecuteAsync);

    /// <inheritdoc/>
    public async Task ExecuteAsync(Func<IOperationContext, Task> operation)
    {
        using var ctx = _contextFactory();
        try
        {
            _repository.Add(ctx);
            _logger.LogInformation($"Starting operation {ctx.Id}");
            await operation(ctx);
            _logger.LogInformation($"Operation {ctx.Id} completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Operation {ctx.Id} completed with errors");
            throw;
        }
        finally
        {
            _logger.LogInformation($"Cleaning up operation {ctx.Id}");
            _repository.Remove(ctx.Id);
        }
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WinGetStudio.Services.Operations.Contracts;
using WinGetStudio.Services.Operations.Models;
using WinGetStudio.Services.Operations.Models.State;

namespace WinGetStudio.Services.Operations.Services;

internal sealed partial class OperationManager
{
    private readonly ILogger<OperationManager> _logger;
    private readonly IOperationPublisher _dispatcher;
    private readonly ConcurrentDictionary<Guid, IOperationContext> _operations;

    public OperationManager(ILogger<OperationManager> logger, IOperationPublisher dispatcher)
    {
        _logger = logger;
        _operations = [];
        _dispatcher = dispatcher;
    }

    public async Task ExecuteAsync(IOperation operation) => await ExecuteAsync(operation.ExecuteAsync);

    public async Task ExecuteAsync(Func<IOperationContext, Task> operation)
    {
        using var ctx = new OperationContext(this);
        try
        {
            _operations.TryAdd(ctx.Id, null!);
            _logger.LogInformation($"Starting operation {ctx.Id}");
            await operation(ctx);
            _logger.LogInformation($"Operation {ctx.Id} completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Operation {ctx.Id} completed with an error");
            throw;
        }
        finally
        {
            _logger.LogInformation($"Cleaning up operation {ctx.Id}");
            _operations.TryRemove(ctx.Id, out _);
        }
    }

    public void PublishSnapshot(Guid id)
    {
        // Publish the snapshot if the operation exists
        if (_operations.TryGetValue(id, out var ctx))
        {
            _dispatcher.PublishSnapshots(ctx.CurrentSnapshot);

            // Also publish the global activity
            PublishGlobalActivity();
        }
        else
        {
            _logger.LogWarning($"Attempted to publish snapshot for non-existent operation {id}");
        }
    }

    public void PublishEvent(Guid id)
    {
        // Publish the event if the operation exists
        if (_operations.TryGetValue(id, out var ctx))
        {
            _dispatcher.PublishEvent(ctx.CurrentSnapshot.Properties);
        }
        else
        {
            _logger.LogWarning($"Attempted to publish event for non-existent operation {id}");
        }
    }
}

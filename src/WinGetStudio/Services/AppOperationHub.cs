// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Models.Operations;
using WinGetStudio.Services.Operations.Contracts;
using WinGetStudio.Services.Operations.Models;
using WinGetStudio.Services.Operations.Models.Policies;
using WinGetStudio.Services.Operations.Models.States;

namespace WinGetStudio.Services;

public sealed partial class AppOperationHub : IAppOperationHub
{
    private readonly IOperationHub _operationHub;
    private readonly ILogger _logger;

    public static OperationExecutionOptions InteractiveOptions { get; } = new()
    {
        Policies =
        [
            new AutoCompletePolicy(),
            new SnapshotRetentionPolicy(OperationStatus.Completed, OperationSeverity.Success, TimeSpan.FromSeconds(3))
        ],
    };

    public static OperationExecutionOptions PassiveOptions { get; } = new();

    public AppOperationHub(IOperationHub operationHub, ILogger<AppOperationHub> logger)
    {
        _operationHub = operationHub;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<OperationResult<TResult>> ExecuteAsync<TResult>(OperationExecutionOptions options, Func<IOperationContext, Task<OperationResult<TResult>>> operation)
    {
        try
        {
            return await _operationHub.ExecuteAsync(operation, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled error occurred while executing an operation.");
            return new() { Error = ex };
        }
    }

    /// <inheritdoc/>
    public async Task ExecuteAsync(OperationExecutionOptions options, Func<IOperationContext, Task> operation)
    {
        try
        {
            await _operationHub.ExecuteAsync(operation, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled error occurred while executing an operation.");
        }
    }

    /// <inheritdoc/>
    public async Task<OperationResult<TResult>> ExecuteAsync<TResult>(OperationExecutionOptions options, IOperation<OperationResult<TResult>> operation)
    {
        try
        {
            return await _operationHub.ExecuteAsync(operation, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled error occurred while executing an operation.");
            return new() { Error = ex };
        }
    }
}

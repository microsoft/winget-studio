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
    private readonly IOperationFactory _operationFactory;

    /// <inheritdoc/>
    public IEventStream<IReadOnlyList<OperationSnapshot>> Snapshots => _operationHub.Snapshots;

    /// <inheritdoc/>
    public IEventStream<OperationNotification> Notifications => _operationHub.Notifications;

    /// <inheritdoc/>
    public IEventStream<GlobalActivity> GlobalActivity => _operationHub.GlobalActivity;

    public static OperationExecutionOptions InteractiveOptions { get; } = new()
    {
        Policies =
        [
            new AutoCompletePolicy(),
            new SnapshotRetentionPolicy(OperationStatus.Completed, OperationSeverity.Success, TimeSpan.FromSeconds(3))
        ],
    };

    public static OperationExecutionOptions PassiveOptions { get; } = new();

    public AppOperationHub(IOperationHub operationHub, IOperationFactory operationFactory, ILogger<AppOperationHub> logger)
    {
        _operationHub = operationHub;
        _operationFactory = operationFactory;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<OperationResult<TResult>> ExecuteAsync<TResult>(OperationExecutionOptions options, Func<IOperationContext, IOperationFactory, Task<OperationResult<TResult>>> operation)
    {
        try
        {
            return await _operationHub.ExecuteAsync(context => operation(context, _operationFactory), options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled error occurred while executing an operation.");
            return new() { Error = ex };
        }
    }

    /// <inheritdoc/>
    public async Task ExecuteAsync(OperationExecutionOptions options, Func<IOperationContext, IOperationFactory, Task> operation)
    {
        try
        {
            await _operationHub.ExecuteAsync(context => operation(context, _operationFactory), options);
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

    /// <inheritdoc/>
    public Task<IOperationScope> BeginOperationAsync(OperationExecutionOptions? options = null, CancellationToken cancellationToken = default) => _operationHub.BeginOperationAsync(options, cancellationToken);

    /// <inheritdoc/>
    public void StopSnapshotBroadcast(Guid id) => _operationHub.StopSnapshotBroadcast(id);
}

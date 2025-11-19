// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Models;
using WinGetStudio.Models.Operations;
using WinGetStudio.Services.Operations.Contracts;
using WinGetStudio.Services.Operations.Extensions;
using WinGetStudio.Services.Operations.Models;
using WinGetStudio.Services.Operations.Models.Policies;
using WinGetStudio.Services.Operations.Models.States;

namespace WinGetStudio.Services;

public sealed partial class AppOperationHub : IAppOperationHub
{
    private readonly ILogger<AppOperationHub> _logger;
    private readonly IStringLocalizer<AppOperationHub> _localizer;
    private readonly IOperationHub _operationHub;
    private readonly IOperationFactory _operationFactory;
    private readonly OperationExecutionOptions _options;
    private readonly TimeSpan _successSnapshotRetention = TimeSpan.FromSeconds(3);

    /// <inheritdoc/>
    public IEventStream<IReadOnlyList<OperationSnapshot>> Snapshots => _operationHub.Snapshots;

    /// <inheritdoc/>
    public IEventStream<OperationNotification> Notifications => _operationHub.Notifications;

    /// <inheritdoc/>
    public IEventStream<GlobalActivity> GlobalActivity => _operationHub.GlobalActivity;

    public AppOperationHub(
        ILogger<AppOperationHub> logger,
        IStringLocalizer<AppOperationHub> localizer,
        IOperationHub operationHub,
        IOperationFactory operationFactory)
    {
        _logger = logger;
        _localizer = localizer;
        _operationHub = operationHub;
        _operationFactory = operationFactory;
        _options = new()
        {
            NotifyOnCompletion = true,
            Policies =
            [
                new AutoCompletePolicy(),
                new ViewLogsOnFailurePolicy(_localizer["ActivityPane_ViewLogsText"]),

                // For successful operations, retain snapshots for a short
                // period to allow UI to show completion state before removing them.
                new SnapshotRetentionPolicy(
                    props => props.Status == OperationStatus.Completed && props.Severity == OperationSeverity.Success,
                    _successSnapshotRetention),
            ],
        };
    }

    /// <inheritdoc/>
    public async Task RunAsync(Func<IOperationContext, IOperationFactory, Task> operation)
    {
        try
        {
            await _operationHub.ExecuteAsync(
                async context =>
                {
                    context.Start();
                    await operation(context, _operationFactory);
                },
                _options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled error occurred while executing an operation.");
        }
    }

    /// <inheritdoc/>
    public async Task RunWithProgressAsync(Func<OperationProperties, OperationProperties> mutate, Func<IOperationContext, IOperationFactory, Task> operation, bool canCancel = true)
    {
        _ = await RunWithProgressAsync<Task>(mutate, async (context, factory) =>
        {
            await operation(context, factory);
            return new() { Result = Task.CompletedTask };
        });
    }

    public async Task<OperationResult<TResult>> RunWithProgressAsync<TResult>(Func<OperationProperties, OperationProperties> mutate, Func<IOperationContext, IOperationFactory, Task<OperationResult<TResult>>> operation, bool canCancel = true)
    {
        try
        {
            return await _operationHub.ExecuteAsync(
                async context =>
                {
                    context.Start(mutate);
                    if (canCancel)
                    {
                        context.AddCancelAction(_localizer["ActivityPane_CancelText"]);
                    }

                    _operationHub.StartSnapshotBroadcast(context.CurrentSnapshot);
                    return await operation(context, _operationFactory);
                },
                _options);
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

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WinGetStudio.Services.Operations.Contracts;
using WinGetStudio.Services.Operations.Models;
using WinGetStudio.Services.Operations.Models.States;

namespace WinGetStudio.Services.Operations.Services;

internal sealed partial class OperationManager : IOperationManager
{
    private readonly ILogger<OperationManager> _logger;
    private readonly IOperationRepository _repository;
    private readonly IOperationPublisher _publisher;
    private readonly IOperationPolicyManager _policyManager;
    private readonly object _contextLock = new();
    private readonly object _snapshotLock = new();

    public OperationManager(
        ILogger<OperationManager> logger,
        IOperationRepository repository,
        IOperationPublisher publisher,
        IOperationPolicyManager policyManager)
    {
        _logger = logger;
        _repository = repository;
        _publisher = publisher;
        _policyManager = policyManager;
    }

    /// <inheritdoc/>
    public void PublishNotification(OperationNotification notification)
    {
        _logger.LogInformation($"Publishing operation notification for operation id: {notification.OperatinoId}");
        _publisher.PublishNotification(notification);
    }

    /// <inheritdoc/>
    public void AddActiveOperationContext(OperationContext context)
    {
        _logger.LogInformation($"Adding active operation context for operation id: {context.Id}");
        lock (_contextLock)
        {
            _repository.AddActiveOperationContext(context);
        }

        PublishGlobalActivity();
    }

    /// <inheritdoc/>
    public void RemoveActiveOperationContext(Guid id)
    {
        _logger.LogInformation($"Removing active operation context for operation id: {id}");
        lock (_contextLock)
        {
            _repository.RemoveActiveOperationContext(id);
        }

        PublishGlobalActivity();
    }

    /// <inheritdoc/>
    public void AddOperationSnapshot(OperationSnapshot snapshot)
    {
        lock (_snapshotLock)
        {
            if (_repository.ContainsOperationSnapshot(snapshot.Id))
            {
                _logger.LogInformation($"Snapshot for operation id: {snapshot.Id} already exists in repository. Skipping add.");
                return;
            }

            _logger.LogInformation($"Adding or updating snapshot for operation id: {snapshot.Id} in repository.");
            _repository.AddOperationSnapshot(snapshot);
        }

        PublishSnapshots();
    }

    /// <inheritdoc/>
    public void RemoveOperationSnapshot(Guid id)
    {
        lock (_snapshotLock)
        {
            if (!_repository.ContainsOperationSnapshot(id))
            {
                _logger.LogInformation($"Snapshot for operation id: {id} does not exist in repository. Skipping remove.");
                return;
            }

            _logger.LogInformation($"Removing snapshot for operation id: {id} in repository.");
            _repository.RemoveOperationSnapshot(id);
        }

        PublishSnapshots();
    }

    /// <inheritdoc/>
    public void UpdateOperationSnapshot(OperationSnapshot snapshot)
    {
        var repoUpdated = false;
        lock (_snapshotLock)
        {
            if (_repository.ContainsOperationSnapshot(snapshot.Id))
            {
                _logger.LogInformation($"Updating snapshot for operation id: {snapshot.Id} in repository.");
                _repository.UpdateOperationSnapshot(snapshot);
                repoUpdated = true;
            }
            else
            {
                _logger.LogInformation($"Snapshot for operation id: {snapshot.Id} is not updated in repository because it is not broadcasted.");
            }
        }

        PublishGlobalActivity();
        if (repoUpdated)
        {
            PublishSnapshots();
        }
    }

    /// <inheritdoc/>
    public Task ApplyStartPoliciesAsync(IReadOnlyList<IOperationPolicy>? policies, IOperationContext context)
    {
        _logger.LogInformation($"Applying start policies for operation id: {context.Id}");
        return _policyManager.ApplyPoliciesAsync<IOperationStartPolicy>(policies, context);
    }

    /// <inheritdoc/>
    public Task ApplyCompletionPoliciesAsync(IReadOnlyList<IOperationPolicy>? policies, IOperationContext context)
    {
        _logger.LogInformation($"Applying completion policies for operation id: {context.Id}");
        return _policyManager.ApplyPoliciesAsync<IOperationCompletionPolicy>(policies, context);
    }

    /// <summary>
    /// Publishes the current operation snapshots.
    /// </summary>
    private void PublishSnapshots()
    {
        _logger.LogInformation("Publishing operation snapshots.");
        List<OperationSnapshot> snapshots = [];
        lock (_snapshotLock)
        {
            snapshots.AddRange(_repository.OperationSnapshots);
        }

        _publisher.PublishSnapshots(snapshots);
    }

    /// <summary>
    /// Publishes the current global activity.
    /// </summary>
    private void PublishGlobalActivity()
    {
        _logger.LogInformation("Publishing global activity.");
        List<OperationContext> contexts = [];
        lock (_contextLock)
        {
            contexts.AddRange(_repository.ActiveOperationContexts);
        }

        _publisher.PublishGlobalActivity(contexts);
    }
}

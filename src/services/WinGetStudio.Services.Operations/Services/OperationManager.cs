// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using WinGetStudio.Services.Operations.Contracts;
using WinGetStudio.Services.Operations.Models;
using WinGetStudio.Services.Operations.Models.State;

namespace WinGetStudio.Services.Operations.Services;

internal sealed partial class OperationManager : IOperationManager
{
    private readonly IOperationRepository _repository;
    private readonly IOperationPublisher _publisher;
    private readonly object _lock = new();

    public OperationManager(IOperationRepository repository, IOperationPublisher publisher)
    {
        _repository = repository;
        _publisher = publisher;
    }

    /// <inheritdoc/>
    public void Publish(OperationContext context)
    {
        lock (_lock)
        {
            _repository.Add(context);
        }
    }

    /// <inheritdoc/>
    public void Unpublish(OperationContext context)
    {
        lock (_lock)
        {
            _repository.Remove(context);
        }
    }

    /// <inheritdoc/>
    public void PublishSnapshots()
    {
        List<OperationSnapshot> snapshots = [];
        lock (_lock)
        {
            snapshots.AddRange(_repository.Snapshots);
        }

        _publisher.PublishSnapshots(snapshots);
    }

    /// <inheritdoc/>
    public void PublishNotification(OperationNotification notification)
    {
        _publisher.PublishNotification(notification);
    }
}

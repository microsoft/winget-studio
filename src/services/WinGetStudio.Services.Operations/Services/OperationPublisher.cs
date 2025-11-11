// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using WinGetStudio.Services.Operations.Contracts;
using WinGetStudio.Services.Operations.Models;
using WinGetStudio.Services.Operations.Models.State;

namespace WinGetStudio.Services.Operations.Services;

internal sealed partial class OperationPublisher : IOperationPublisher
{
    private readonly IOperationRepository _repository;

    /// <inheritdoc/>
    public EventStream<IReadOnlyList<OperationSnapshot>> Snapshots { get; } = new();

    /// <inheritdoc/>
    public EventStream<GlobalActivity> GlobalActivity { get; } = new();

    /// <inheritdoc/>
    public EventStream<OperationProperties> Events { get; } = new();

    public OperationPublisher(IOperationRepository repository)
    {
        _repository = repository;
    }

    /// <inheritdoc/>
    public void PublishSnapshots()
    {
        Snapshots.Publish(_repository.Snapshots);
        PublishGlobalActivity();
    }

    /// <inheritdoc/>
    public void PublishEvent(OperationProperties properties) => Events.Publish(properties);

    /// <summary>
    /// Publishes the global activity based on the current running operations.
    /// </summary>
    private void PublishGlobalActivity()
    {
        List<OperationContext> runningOps = [.._repository.Operations.Where(op => op.CurrentSnapshot.Properties.Status == OperationStatus.Running)];
        if (runningOps.Count == 0)
        {
            // If no operations are running, publish zero progress
            GlobalActivity.Publish(new(Percent: null, InProgressCount: 0));
        }
        else if (runningOps.Count == 1)
        {
            // If exactly one operation is running, publish its progress
            var props = runningOps[0].CurrentSnapshot.Properties;
            GlobalActivity.Publish(new(Percent: props.Percent, InProgressCount: 1));
        }
        else
        {
            // If multiple operations are running, publish null progress with the count
            GlobalActivity.Publish(new(Percent: null, InProgressCount: runningOps.Count));
        }
    }
}

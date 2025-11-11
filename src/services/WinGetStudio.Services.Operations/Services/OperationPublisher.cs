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
    /// <inheritdoc/>
    public EventStream<IReadOnlyList<OperationSnapshot>> Snapshots { get; } = new();

    /// <inheritdoc/>
    public EventStream<GlobalActivity> GlobalActivity { get; } = new();

    /// <inheritdoc/>
    public EventStream<OperationNotification> Notifications { get; } = new();

    /// <inheritdoc/>
    public void PublishSnapshots(IReadOnlyList<OperationSnapshot> snapshots)
    {
        Snapshots.Publish(snapshots);
        PublishGlobalActivity(snapshots);
    }

    /// <inheritdoc/>
    public void PublishNotification(OperationNotification notification) => Notifications.Publish(notification);

    /// <summary>
    /// Publishes the global activity based on the current running operations.
    /// </summary>
    /// <param name="snapshots">The current operation snapshots.</param>
    private void PublishGlobalActivity(IReadOnlyList<OperationSnapshot> snapshots)
    {
        List<OperationSnapshot> inProgress = [..snapshots.Where(os => os.Properties.Status == OperationStatus.Running)];
        if (inProgress.Count == 0)
        {
            // If no operations are running, publish zero progress
            GlobalActivity.Publish(new(Percent: null, InProgressCount: 0));
        }
        else if (inProgress.Count == 1)
        {
            // If exactly one operation is running, publish its progress
            var props = inProgress[0].Properties;
            GlobalActivity.Publish(new(Percent: props.Percent, InProgressCount: 1));
        }
        else
        {
            // If multiple operations are running, publish null progress with the count
            GlobalActivity.Publish(new(Percent: null, InProgressCount: inProgress.Count));
        }
    }
}

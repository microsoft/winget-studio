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
    public EventStream<IReadOnlyList<OperationSnapshot>> Snapshots { get; } = new();

    public EventStream<GlobalActivity> GlobalActivity { get; } = new();

    public EventStream<OperationProperties> Events { get; } = new();

    public void PublishSnapshots(IReadOnlyList<OperationSnapshot> snapshots)
    {
        Snapshots.Publish(snapshots);
    }

    public void PublishEvent(OperationProperties properties) => Events.Publish(properties);

    private void PublishGlobalActivity(IReadOnlyList<OperationSnapshot> snapshots)
    {
        List<OperationSnapshot> runningOps = [..snapshots.Where(s => s.Properties.Status == OperationStatus.Running)];
        if (runningOps.Count == 0)
        {
            GlobalActivity.Publish(new(Percent: null, InProgressCount: 0));
        }
        else if (runningOps.Count == 1)
        {
            var op = runningOps[0];
            var props = op.CurrentSnapshot.Properties;
            GlobalActivity.Publish(new(Percent: props.Percent, InProgressCount: 1));
        }
        else
        {
            _dispatcher.PublishActivity(new(null, runningOps.Count));
        }
    }
}

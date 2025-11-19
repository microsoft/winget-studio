// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using WinGetStudio.Services.Operations.Contracts;
using WinGetStudio.Services.Operations.Models.States;

namespace WinGetStudio.Services.Operations.Models.Policies;

public sealed partial class SnapshotRetentionPolicy : IOperationCompletionPolicy
{
    private readonly Func<OperationProperties, bool> _applyCondition;
    private readonly TimeSpan _retentionPeriod;

    public SnapshotRetentionPolicy(Func<OperationProperties, bool> applyCondition, TimeSpan retentionPeriod)
    {
        _applyCondition = applyCondition;
        _retentionPeriod = retentionPeriod;
    }

    /// <inheritdoc/>
    public bool CanApply(IOperationContext context)
    {
        return _applyCondition(context.CurrentSnapshot.Properties);
    }

    /// <inheritdoc/>
    public Task ApplyAsync(IOperationContext context)
    {
        // In a background task, wait for the retention period then stop
        // broadcasting snapshots.
        _ = Task.Run(async () =>
        {
            await Task.Delay(_retentionPeriod);
            context.StopSnapshotBroadcast();
        });

        return Task.CompletedTask;
    }
}

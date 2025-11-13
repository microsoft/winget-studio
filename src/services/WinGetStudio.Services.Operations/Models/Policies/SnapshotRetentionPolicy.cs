// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using WinGetStudio.Services.Operations.Contracts;
using WinGetStudio.Services.Operations.Models.States;

namespace WinGetStudio.Services.Operations.Models.Policies;

public sealed partial class SnapshotRetentionPolicy : IOperationCompletionPolicy
{
    private readonly OperationStatus _status;
    private readonly OperationSeverity _severity;
    private readonly TimeSpan _retentionPeriod;

    public SnapshotRetentionPolicy(OperationStatus status, OperationSeverity severity, TimeSpan retentionPeriod)
    {
        _status = status;
        _severity = severity;
        _retentionPeriod = retentionPeriod;
    }

    /// <inheritdoc/>
    public bool CanApply(IOperationContext context)
    {
        var status = context.CurrentSnapshot.Properties.Status;
        var severity = context.CurrentSnapshot.Properties.Severity;
        return status == _status && severity == _severity;
    }

    /// <inheritdoc/>
    public async Task ApplyAsync(IOperationContext context)
    {
        await Task.Delay(_retentionPeriod);
        context.StopSnapshotBroadcast();
    }
}

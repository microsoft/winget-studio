// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using WinGetStudio.Services.Operations.Contracts;

namespace WinGetStudio.Services.Operations.Models.Policies;

public sealed partial class AutoStopSnapshotBroadcastPolicy : IOperationCompletionPolicy
{
    public bool CanApply(IOperationContext context) => true;

    public Task ApplyAsync(IOperationContext context)
    {
        context.StopSnapshotBroadcast();
        return Task.CompletedTask;
    }
}

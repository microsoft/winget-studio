// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using WinGetStudio.Services.Operations.Contracts;
using WinGetStudio.Services.Operations.Extensions;
using WinGetStudio.Services.Operations.Models.States;

namespace WinGetStudio.Services.Operations.Models.Policies;

public sealed partial class OperationAutoStartPolicy : IOperationStartPolicy
{
    /// <inheritdoc/>
    public bool CanApply(IOperationContext context)
    {
        return context.CurrentSnapshot.Properties.Status == OperationStatus.NotStarted;
    }

    /// <inheritdoc/>
    public Task ApplyAsync(IOperationContext context)
    {
        context.Start();
        return Task.CompletedTask;
    }
}

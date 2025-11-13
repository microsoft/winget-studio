// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using WinGetStudio.Services.Operations.Contracts;
using WinGetStudio.Services.Operations.Extensions;

namespace WinGetStudio.Services.Operations.Models.Policies;

internal sealed partial class AutoCompletePolicy : IOperationCompletionPolicy
{
    /// <inheritdoc/>
    public bool CanApply(IOperationContext context)
    {
        return !context.CurrentSnapshot.Properties.IsTerminated;
    }

    /// <inheritdoc/>
    public async Task ApplyAsync(IOperationContext context)
    {
        context.Complete();
        await Task.CompletedTask;
    }
}

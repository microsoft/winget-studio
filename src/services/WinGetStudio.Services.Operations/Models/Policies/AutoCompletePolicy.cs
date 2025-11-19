// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using WinGetStudio.Services.Operations.Contracts;
using WinGetStudio.Services.Operations.Extensions;
using WinGetStudio.Services.Operations.Models.States;

namespace WinGetStudio.Services.Operations.Models.Policies;

public sealed partial class AutoCompletePolicy : IOperationCompletionPolicy
{
    private readonly OperationSeverity? _severity;

    public AutoCompletePolicy(OperationSeverity? severity = null)
    {
        _severity = severity;
    }

    /// <inheritdoc/>
    public bool CanApply(IOperationContext context)
    {
        return !context.CurrentSnapshot.Properties.IsTerminated;
    }

    /// <inheritdoc/>
    public async Task ApplyAsync(IOperationContext context)
    {
        var severity = _severity ?? context.CurrentSnapshot.Properties.Severity;
        context.Complete(props => props with { Severity = severity });
        await Task.CompletedTask;
    }
}

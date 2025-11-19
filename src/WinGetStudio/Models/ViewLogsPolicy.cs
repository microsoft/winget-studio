// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Windows.System;
using WinGetStudio.Services.Core.Helpers;
using WinGetStudio.Services.Operations.Contracts;
using WinGetStudio.Services.Operations.Models.States;

namespace WinGetStudio.Models;

public sealed partial class ViewLogsOnFailurePolicy : IOperationCompletionPolicy
{
    private readonly string _text;

    public ViewLogsOnFailurePolicy(string text)
    {
        _text = text;
    }

    public bool CanApply(IOperationContext context)
    {
        var props = context.CurrentSnapshot.Properties;
        return props.Status == OperationStatus.Completed && props.Severity == OperationSeverity.Error;
    }

    public async Task ApplyAsync(IOperationContext context)
    {
        var logsAction = new OperationAction(_text, true, async () =>
        {
            await Launcher.LaunchUriAsync(new Uri(RuntimeHelper.GetAppInstanceLogPath()));
        });
        context.CommitSnapshot(props => props with { Actions = [.. props.Actions, logsAction] });
    }
}

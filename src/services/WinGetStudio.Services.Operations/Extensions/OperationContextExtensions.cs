// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using WinGetStudio.Services.Operations.Contracts;
using WinGetStudio.Services.Operations.Models.State;

namespace WinGetStudio.Services.Operations.Extensions;

public static partial class OperationContextExtensions
{
    public static void ReportProgress(this IOperationContext ctx, int percent, Func<OperationProperties, OperationProperties>? mutate = null)
    {
        ctx.UpdateInternal(props => props with { Percent = percent }, mutate);
    }

    public static void ReportIndeterminate(this IOperationContext ctx, Func<OperationProperties, OperationProperties>? mutate = null)
    {
        ctx.UpdateInternal(props => props with { Percent = null }, mutate);
    }

    public static void SetText(this IOperationContext ctx, string title, string message, Func<OperationProperties, OperationProperties>? mutate = null)
    {
        ctx.UpdateInternal(props => props with { Title = title, Message = message }, mutate);
    }

    public static void SetText(this IOperationContext ctx, string message, Func<OperationProperties, OperationProperties>? mutate = null)
    {
        ctx.UpdateInternal(props => props with { Message = message }, mutate);
    }

    public static void Complete(this IOperationContext ctx, Func<OperationProperties, OperationProperties>? mutate = null)
    {
        ctx.UpdateInternal(
            props => props with
            {
                Status = OperationStatus.Completed,
                Percent = 100,
                Actions = [],
            },
            mutate);
    }

    public static void Success(this IOperationContext ctx, Func<OperationProperties, OperationProperties>? mutate = null)
    {
        ctx.UpdateInternal(
            props => props with
            {
                Status = OperationStatus.Completed,
                Percent = 100,
                Severity = OperationSeverity.Success,
                Actions = [],
            },
            mutate);
    }

    public static void Fail(this IOperationContext ctx, Func<OperationProperties, OperationProperties>? mutate = null)
    {
        ctx.UpdateInternal(
            props => props with
            {
                Status = OperationStatus.Completed,
                Percent = 100,
                Severity = OperationSeverity.Error,
                Actions = [],
            },
            mutate);
    }

    public static void CancelAndUpdate(this IOperationContext ctx, Func<OperationProperties, OperationProperties>? mutate = null)
    {
        ctx.Cancel();
        ctx.UpdateInternal(
            props => props with
            {
                Status = OperationStatus.Canceled,
                Percent = 0,
                Severity = OperationSeverity.Warning,
                Actions = [],
            },
            mutate);
    }

    public static void AddCancelAction(this IOperationContext ctx, string text, bool isPrimary = true)
    {
        var cancelAction = new OperationAction(text, isPrimary, () => Task.FromResult(ctx.CancelAndUpdate));
        ctx.Update(props => props with { Actions = [.. props.Actions, cancelAction] });
    }

    /// <summary>
    /// Update with optional user mutate.
    /// </summary>
    /// <param name="ctx">The operation context.</param>
    /// <param name="updateMutate">The update mutate.</param>
    /// <param name="userMutate">The user mutate.</param>
    private static void UpdateInternal(this IOperationContext ctx, Func<OperationProperties, OperationProperties> updateMutate, Func<OperationProperties, OperationProperties>? userMutate)
    {
        if (userMutate != null)
        {
            ctx.Update(props => userMutate(updateMutate(props)));
        }
        else
        {
            ctx.Update(updateMutate);
        }
    }
}

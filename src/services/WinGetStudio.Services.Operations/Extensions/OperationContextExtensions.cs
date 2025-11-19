// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using WinGetStudio.Services.Operations.Contracts;
using WinGetStudio.Services.Operations.Models.States;

namespace WinGetStudio.Services.Operations.Extensions;

public static partial class OperationContextExtensions
{
    public static void ReportProgress(this IOperationContext ctx, int percent, Func<OperationProperties, OperationProperties>? mutate = null)
    {
        ctx.CommitSnapshotInternal(props => props with { Percent = percent }, mutate);
    }

    public static void ReportIndeterminate(this IOperationContext ctx, Func<OperationProperties, OperationProperties>? mutate = null)
    {
        ctx.CommitSnapshotInternal(props => props with { Percent = null }, mutate);
    }

    public static void SetText(this IOperationContext ctx, string title, string message, Func<OperationProperties, OperationProperties>? mutate = null)
    {
        ctx.CommitSnapshotInternal(props => props with { Title = title, Message = message }, mutate);
    }

    public static void SetText(this IOperationContext ctx, string message, Func<OperationProperties, OperationProperties>? mutate = null)
    {
        ctx.CommitSnapshotInternal(props => props with { Message = message }, mutate);
    }

    public static void Start(this IOperationContext ctx, Func<OperationProperties, OperationProperties>? mutate = null)
    {
        ctx.CommitSnapshotInternal(props => props with { Status = OperationStatus.Running }, mutate);
    }

    public static void Complete(this IOperationContext ctx, Func<OperationProperties, OperationProperties>? mutate = null)
    {
        ctx.CommitSnapshotInternal(
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
        ctx.CommitSnapshotInternal(
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
        ctx.CommitSnapshotInternal(
            props => props with
            {
                Status = OperationStatus.Completed,
                Percent = 100,
                Severity = OperationSeverity.Error,
                Actions = [],
            },
            mutate);
    }

    public static void Warn(this IOperationContext ctx, Func<OperationProperties, OperationProperties>? mutate = null)
    {
        ctx.CommitSnapshotInternal(
            props => props with
            {
                Status = OperationStatus.Completed,
                Percent = 100,
                Severity = OperationSeverity.Warning,
                Actions = [],
            },
            mutate);
    }

    public static void Canceled(this IOperationContext ctx, Func<OperationProperties, OperationProperties>? mutate = null)
    {
        ctx.CommitSnapshotInternal(
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
        var cancelAction = new OperationAction(text, isPrimary, () =>
        {
            ctx.RequestCancellation();
            return Task.CompletedTask;
        });
        ctx.CommitSnapshot(props => props with { Actions = [.. props.Actions, cancelAction] });
    }

    public static void AddDoneAction(this IOperationContext ctx, string text, bool isPrimary = true)
    {
        var cancelAction = new OperationAction(text, isPrimary, () =>
        {
            ctx.StopSnapshotBroadcast();
            return Task.CompletedTask;
        });
        ctx.CommitSnapshot(props => props with { Actions = [.. props.Actions, cancelAction] });
    }

    /// <summary>
    /// Commit snapshot internal helper to handle optional user mutate.
    /// </summary>
    /// <param name="ctx">The operation context.</param>
    /// <param name="updateMutate">The update mutate.</param>
    /// <param name="userMutate">The user mutate.</param>
    private static void CommitSnapshotInternal(this IOperationContext ctx, Func<OperationProperties, OperationProperties> updateMutate, Func<OperationProperties, OperationProperties>? userMutate)
    {
        if (userMutate != null)
        {
            ctx.CommitSnapshot(props => userMutate(updateMutate(props)));
        }
        else
        {
            ctx.CommitSnapshot(updateMutate);
        }
    }
}

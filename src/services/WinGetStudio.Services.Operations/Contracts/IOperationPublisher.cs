// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using WinGetStudio.Services.Operations.Models;
using WinGetStudio.Services.Operations.Models.States;

namespace WinGetStudio.Services.Operations.Contracts;

internal interface IOperationPublisher
{
    /// <summary>
    /// Gets the snapshots event stream.
    /// </summary>
    EventStream<IReadOnlyList<OperationSnapshot>> Snapshots { get; }

    /// <summary>
    /// Gets the global activity event stream.
    /// </summary>
    EventStream<GlobalActivity> GlobalActivity { get; }

    /// <summary>
    /// Gets the operation notifications event stream.
    /// </summary>
    EventStream<OperationNotification> Notifications { get; }

    /// <summary>
    /// Publishes the given operation snapshots.
    /// </summary>
    /// <param name="snapshots">The snapshots to publish.</param>
    void PublishSnapshots(IReadOnlyList<OperationSnapshot> snapshots);

    /// <summary>
    /// Publishes an operation notification.
    /// </summary>
    /// <param name="notification">The notification to publish.</param>
    void PublishNotification(OperationNotification notification);
}

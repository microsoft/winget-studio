// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.Services.Operations.Models;
using WinGetStudio.Services.Operations.Models.State;

namespace WinGetStudio.Services.Operations.Contracts;

internal interface IOperationManager
{
    /// <summary>
    /// Publishes the operation context to the repository.
    /// </summary>
    /// <param name="context">The operation context to publish.</param>
    void Publish(OperationContext context);

    /// <summary>
    /// Unpublishes the operation context from the repository.
    /// </summary>
    /// <param name="context">The operation context to unpublish.</param>
    void Unpublish(OperationContext context);

    /// <summary>
    /// Publishes all operation snapshots to subscribers.
    /// </summary>
    void PublishSnapshots();

    /// <inheritdoc cref="IOperationPublisher.PublishNotification(OperationNotification)"/>
    void PublishNotification(OperationNotification notification);
}

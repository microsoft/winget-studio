// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.Services.Operations.Models;
using WinGetStudio.Services.Operations.Models.States;

namespace WinGetStudio.Services.Operations.Contracts;

internal interface IOperationManager
{
    /// <summary>
    /// Register the operation context to the repository.
    /// </summary>
    /// <param name="context">The operation context to register.</param>
    void Register(OperationContext context);

    /// <summary>
    /// Unregister the operation context from the repository.
    /// </summary>
    /// <param name="context">The operation context to unregister.</param>
    void Unregister(OperationContext context);

    /// <summary>
    /// Publishes all operation snapshots to subscribers.
    /// </summary>
    void PublishSnapshots();

    /// <inheritdoc cref="IOperationPublisher.PublishNotification(OperationNotification)"/>
    void PublishNotification(OperationNotification notification);
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WinGetStudio.Services.Operations.Models;
using WinGetStudio.Services.Operations.Models.States;

namespace WinGetStudio.Services.Operations.Contracts;

internal interface IOperationManager
{
    /// <inheritdoc cref="IOperationPublisher.PublishNotification(OperationNotification)"/>
    void PublishNotification(OperationNotification notification);

    /// <inheritdoc cref="IOperationRepository.AddActiveOperationContext(OperationContext)"/>
    void AddActiveOperationContext(OperationContext context);

    /// <inheritdoc cref="IOperationRepository.RemoveActiveOperationContext(Guid)"/>
    void RemoveActiveOperationContext(Guid id);

    /// <inheritdoc cref="IOperationRepository.AddOperationSnapshot(OperationSnapshot)(OperationSnapshot)"/>
    void AddOperationSnapshot(OperationSnapshot snapshot);

    /// <inheritdoc cref="IOperationRepository.RemoveOperationSnapshot(Guid)"/>
    void RemoveOperationSnapshot(Guid snapshot);

    /// <inheritdoc cref="IOperationRepository.UpdateOperationSnapshot(OperationSnapshot)"/>
    void UpdateOperationSnapshot(OperationSnapshot snapshot);

    /// <summary>
    /// Applies the start policies.
    /// </summary>
    /// <param name="policies">The policies to apply.</param>
    /// <param name="context">The operation context.</param>
    Task ApplyStartPoliciesAsync(IReadOnlyList<IOperationPolicy>? policies, IOperationContext context);

    /// <summary>
    /// Applies the completion policies.
    /// </summary>
    /// <param name="policies">The policies to apply.</param>
    /// <param name="context">The operation context.</param>
    Task ApplyCompletionPoliciesAsync(IReadOnlyList<IOperationPolicy>? policies, IOperationContext context);
}

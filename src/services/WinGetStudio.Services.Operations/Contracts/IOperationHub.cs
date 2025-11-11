// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WinGetStudio.Services.Operations.Models.State;

namespace WinGetStudio.Services.Operations.Contracts;

public interface IOperationHub
{
    /// <inheritdoc cref="IOperationPublisher.Snapshots"/>
    IEventStream<IReadOnlyList<OperationSnapshot>> Snapshots { get; }

    /// <inheritdoc cref="IOperationPublisher.Notifications"/>
    IEventStream<OperationNotification> Notifications { get; }

    /// <inheritdoc cref="IOperationPublisher.GlobalActivity"/>
    IEventStream<GlobalActivity> GlobalActivity { get; }

    /// <inheritdoc cref="IOperationExecutor.ExecuteAsync(IOperation)"/>
    Task ExecuteAsync(IOperation operation);

    /// <inheritdoc cref="IOperationExecutor.ExecuteAsync(Func{IOperationContext, Task})"/>
    Task ExecuteAsync(Func<IOperationContext, Task> operation);
}

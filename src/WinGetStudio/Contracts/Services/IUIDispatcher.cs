// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.WinUI;
using Microsoft.UI.Dispatching;

namespace WinGetStudio.Contracts.Services;

public interface IUIDispatcher
{
    /// <inheritdoc cref="DispatcherQueueExtensions.EnqueueAsync(DispatcherQueue, Action, DispatcherQueuePriority)"/>
    Task EnqueueAsync(Action action, DispatcherQueuePriority priority = DispatcherQueuePriority.Normal);

    /// <inheritdoc cref="DispatcherQueueExtensions.EnqueueAsync(DispatcherQueue, Func{Task}, DispatcherQueuePriority)"/>
    Task EnqueueAsync(Func<Task> func, DispatcherQueuePriority priority = DispatcherQueuePriority.Normal);

    /// <inheritdoc cref="DispatcherQueueExtensions.EnqueueAsync{T}(DispatcherQueue, Func{T}, DispatcherQueuePriority)"/>
    Task<T> EnqueueAsync<T>(Func<T> func, DispatcherQueuePriority priority = DispatcherQueuePriority.Normal);

    /// <inheritdoc cref="DispatcherQueueExtensions.EnqueueAsync{T}(DispatcherQueue, Func{Task{T}}, DispatcherQueuePriority)"/>
    Task<T> EnqueueAsync<T>(Func<Task<T>> func, DispatcherQueuePriority priority = DispatcherQueuePriority.Normal);
}

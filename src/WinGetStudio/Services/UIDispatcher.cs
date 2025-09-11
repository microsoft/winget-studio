// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.WinUI;
using Microsoft.UI.Dispatching;
using WinGetStudio.Contracts.Services;

namespace WinGetStudio.Services;

internal sealed class UIDispatcher : IUIDispatcher
{
    private readonly DispatcherQueue _dispatcherQueue;

    public UIDispatcher(DispatcherQueue dispatcherQueue)
    {
        _dispatcherQueue = dispatcherQueue;
    }

    /// <inheritdoc/>
    public Task EnqueueAsync(Action action, DispatcherQueuePriority priority = DispatcherQueuePriority.Normal)
    {
        return _dispatcherQueue.EnqueueAsync(action, priority);
    }

    /// <inheritdoc/>
    public Task EnqueueAsync(Func<Task> func, DispatcherQueuePriority priority = DispatcherQueuePriority.Normal)
    {
        return _dispatcherQueue.EnqueueAsync(func, priority);
    }

    /// <inheritdoc/>
    public Task<T> EnqueueAsync<T>(Func<T> func, DispatcherQueuePriority priority = DispatcherQueuePriority.Normal)
    {
        return _dispatcherQueue.EnqueueAsync(func, priority);
    }

    /// <inheritdoc/>
    public Task<T> EnqueueAsync<T>(Func<Task<T>> func, DispatcherQueuePriority priority = DispatcherQueuePriority.Normal)
    {
        return _dispatcherQueue.EnqueueAsync(func, priority);
    }
}

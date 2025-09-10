// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using WingetStudio.Services.VisualFeedback.Contracts;
using WingetStudio.Services.VisualFeedback.Models;

namespace WingetStudio.Services.VisualFeedback.Services;

internal sealed class LoadingService : ILoadingService
{
    private readonly object _lock = new();
    private readonly LoadingChange _currentState = new();

    public event EventHandler StateChanged;

    public bool IsVisible => SafeGet(() => _currentState.IsVisible);

    public int ProgressValue => SafeGet(() => _currentState.ProgressValue);

    public bool IsIndeterminate => SafeGet(() => _currentState.IsIndeterminate);

    public void SetVisibility(bool isVisible)
    {
        SafeSet(
            () => _currentState.IsVisible != isVisible,
            () => _currentState.IsVisible = isVisible);
    }

    public void SetProgressValue(int value)
    {
        value = Math.Clamp(value, 0, 100);
        SafeSet(
            () => _currentState.ProgressValue != value,
            () => _currentState.ProgressValue = value);
    }

    public void SetIndeterminate(bool isIndeterminate)
    {
        SafeSet(
            () => _currentState.IsIndeterminate != isIndeterminate,
            () => _currentState.IsIndeterminate = isIndeterminate);
    }

    private T SafeGet<T>(Func<T> getter)
    {
        lock (_lock)
        {
            return getter();
        }
    }

    public void SafeSet(Func<bool> condition, Action setter)
    {
        EventHandler handler = null;
        lock (_lock)
        {
            if (condition())
            {
                setter();
                handler = StateChanged;
            }
        }

        handler?.Invoke(this, EventArgs.Empty);
    }
}

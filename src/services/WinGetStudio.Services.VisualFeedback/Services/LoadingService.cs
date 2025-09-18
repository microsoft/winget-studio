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

    /// <inheritdoc/>
    public event EventHandler StateChanged;

    /// <inheritdoc/>
    public bool IsVisible => SafeGet(() => _currentState.IsVisible);

    /// <inheritdoc/>
    public int ProgressValue => SafeGet(() => _currentState.ProgressValue);

    /// <inheritdoc/>
    public bool IsIndeterminate => SafeGet(() => _currentState.IsIndeterminate);

    /// <inheritdoc/>
    public void SetVisibility(bool isVisible)
    {
        SafeSet(
            () => _currentState.IsVisible != isVisible,
            () => _currentState.IsVisible = isVisible);
    }

    /// <inheritdoc/>
    public void SetProgressValue(int value)
    {
        value = Math.Clamp(value, 0, 100);
        SafeSet(
            () => _currentState.ProgressValue != value,
            () => _currentState.ProgressValue = value);
    }

    /// <inheritdoc/>
    public void SetIndeterminate(bool isIndeterminate)
    {
        SafeSet(
            () => _currentState.IsIndeterminate != isIndeterminate,
            () => _currentState.IsIndeterminate = isIndeterminate);
    }

    /// <summary>
    /// Thread-safe getter.
    /// </summary>
    /// <typeparam name="T">Type of the value to get.</typeparam>
    /// <param name="getter">Function to get the value.</param>
    /// <returns>The value.</returns>
    private T SafeGet<T>(Func<T> getter)
    {
        lock (_lock)
        {
            return getter();
        }
    }

    /// <summary>
    /// Thread-safe setter that raises StateChanged event if the condition is met.
    /// </summary>
    /// <param name="condition">Function that returns true if the setter should be executed.</param>
    /// <param name="setter">Action that sets the value.</param>
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

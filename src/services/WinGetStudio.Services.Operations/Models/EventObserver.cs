// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace WinGetStudio.Services.Operations.Models;

/// <summary>
/// Represents an observer that handles events of type T.
/// </summary>
/// <typeparam name="T">The type of events to observe.</typeparam>
internal sealed partial class EventObserver<T> : IObserver<T>
{
    private readonly Action<T> _onNext;

    public EventObserver(Action<T> onNext)
    {
        _onNext = onNext;
    }

    /// <summary>
    /// Observes the next event.
    /// </summary>
    /// <param name="value">The event value.</param>
    public void OnNext(T value) => _onNext(value);

    public void OnCompleted()
    {
        // This method is intentionally left blank.
        // End-of-stream handling is not required for this observer.
    }

    public void OnError(Exception error)
    {
        // This method is intentionally left blank.
        // Error handling is not required for this observer.
    }
}

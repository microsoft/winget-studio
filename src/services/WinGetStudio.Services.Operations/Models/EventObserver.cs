// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace WinGetStudio.Services.Operations.Models;

internal sealed partial class EventObserver<T> : IObserver<T>
{
    private readonly Action<T> _onNext;

    public EventObserver(Action<T> onNext)
    {
        _onNext = onNext;
    }

    public void OnNext(T value)
    {
    }

    public void OnCompleted()
    {
        // No implementation needed
    }

    public void OnError(Exception error)
    {
        // No implementation needed
    }
}

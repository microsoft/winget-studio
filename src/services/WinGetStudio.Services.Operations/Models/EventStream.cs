// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using WinGetStudio.Services.Operations.Contracts;

namespace WinGetStudio.Services.Operations.Models;

internal partial class EventStream<T> : IEventStream<T>, IObservable<T>, IDisposable
{
    private readonly object _lock = new();
    private readonly List<IObserver<T>> _observers = [];
    private bool _disposedValue;

    public IDisposable Subscribe(Action<T> onNextAction) => Subscribe(new EventObserver<T>(onNextAction));

    public IDisposable Subscribe(IObserver<T> observer)
    {
        lock (_lock)
        {
            ObjectDisposedException.ThrowIf(_disposedValue, nameof(EventStream<T>));
            _observers.Add(observer);
        }

        return new Unsubscriber(this, observer);
    }

    public void Publish(T value)
    {
        IObserver<T>[] snapshot;
        lock (_lock)
        {
            if (_disposedValue)
            {
                return;
            }

            snapshot = [.._observers];
        }

        foreach (var observer in snapshot)
        {
            observer.OnNext(value);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _observers.Clear();
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private sealed partial class Unsubscriber : IDisposable
    {
        private readonly EventStream<T> _eventStream;
        private readonly IObserver<T> _observer;
        private bool _disposedValue;

        public Unsubscriber(EventStream<T> eventStream, IObserver<T> observer)
        {
            _eventStream = eventStream;
            _observer = observer;
        }

        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    lock (_eventStream._lock)
                    {
                        if (!_eventStream._disposedValue)
                        {
                            _eventStream._observers.Remove(_observer);
                        }
                    }
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

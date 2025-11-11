// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace WinGetStudio.Services.Operations.Contracts;

/// <summary>
/// Represents an event stream that clients can subscribe to.
/// </summary>
/// <typeparam name="T">The type of event data.</typeparam>
public interface IEventStream<T>
{
    /// <summary>
    /// Subscribe to the event stream with the specified action to be invoked on each event.
    /// </summary>
    /// <param name="onNextAction">The action to be invoked on each event.</param>
    /// <returns>A disposable subscription.</returns>
    IDisposable Subscribe(Action<T> onNextAction);
}

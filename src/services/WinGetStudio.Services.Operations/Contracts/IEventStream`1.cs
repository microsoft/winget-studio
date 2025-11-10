// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace WinGetStudio.Services.Operations.Contracts;

public interface IEventStream<T>
{
    IDisposable Subscribe(Action<T> onNextAction);
}

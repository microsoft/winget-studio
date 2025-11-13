// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace WinGetStudio.Services.Operations.Contracts;

public interface IOperationScope : IAsyncDisposable
{
    /// <summary>
    /// Gets the operation context.
    /// </summary>
    IOperationContext Context { get; }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WinGetStudio.Models.Operations;

public sealed partial class OperationResult<T>
{
    /// <summary>
    /// Gets the result of the operation.
    /// </summary>
    public T? Result { get; init; }

    /// <summary>
    /// Gets the error that occurred during the operation, if any.
    /// </summary>
    public Exception? Error { get; init; }

    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess => Error == null;
}

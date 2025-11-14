// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WinGetStudio.Models.Operations;

public sealed partial class DSCOperationResult<T>
{
    /// <summary>
    /// Gets or sets the result of the operation.
    /// </summary>
    public T? Result { get; set; }

    /// <summary>
    /// Gets or sets the error that occurred during the operation, if any.
    /// </summary>
    public Exception? Error { get; set; }

    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess => Error == null;

    public DSCOperationResult(T result)
    {
        Result = result;
    }

    public DSCOperationResult(Exception error)
    {
        Error = error;
    }
}

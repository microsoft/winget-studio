// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WinGetStudio.Exceptions;

internal sealed partial class DSCUnitValidationException : Exception
{
    public DSCUnitValidationException(string message)
        : base(message)
    {
    }
}

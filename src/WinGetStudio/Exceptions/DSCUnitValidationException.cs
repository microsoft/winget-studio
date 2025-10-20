// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WinGetStudio.Exceptions;

/// <summary>
/// Represents an exception that occurs during the validation of a DSC unit.
/// </summary>
internal sealed partial class DSCUnitValidationException : Exception
{
    public DSCUnitValidationException(string message)
        : base(message)
    {
    }
}

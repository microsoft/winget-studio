// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WinGetStudio.CLI.DSCv3.Models;

/// <summary>
/// Specifies the severity level of a message.
/// </summary>
internal enum DscMessageLevel
{
    /// <summary>
    /// Represents an error message.
    /// </summary>
    Error,

    /// <summary>
    /// Represents a warning message.
    /// </summary>
    Warning,

    /// <summary>
    /// Represents an informational message.
    /// </summary>
    Info,

    /// <summary>
    /// Represents a debug message.
    /// </summary>
    Debug,

    /// <summary>
    /// Represents a trace message.
    /// </summary>
    Trace,
}

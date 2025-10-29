// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

internal sealed partial class DSCProcessResult
{
    /// <summary>
    /// Gets the output from the process.
    /// </summary>
    public string Output { get; }

    /// <summary>
    /// Gets the errors from the process.
    /// </summary>
    public string Errors { get; }

    /// <summary>
    /// Gets the exit code from the process.
    /// </summary>
    public int ExitCode { get; }

    /// <summary>
    /// Gets a value indicating whether the process was successful.
    /// </summary>
    public bool IsSuccess => ExitCode == 0;

    public DSCProcessResult(string output, string errors, int existCode)
    {
        Output = output;
        Errors = errors;
        ExitCode = existCode;
    }
}

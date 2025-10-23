// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

internal sealed partial class DSCProcessResult
{
    public string Output { get; }

    public string Errors { get; }

    public int ExitCode { get; }

    public bool IsSuccess => ExitCode == 0;

    public DSCProcessResult(string output, string errors, int existCode)
    {
        Output = output;
        Errors = errors;
        ExitCode = existCode;
    }
}

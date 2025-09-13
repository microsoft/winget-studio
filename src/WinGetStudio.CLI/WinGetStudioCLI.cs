// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.CommandLine;
using Windows.Win32;
using WinGetStudio.CLI.DSCv3.Commands;

namespace WinGetStudio.CLI;

public sealed class WinGetStudioCLI
{
    private const uint AttachParentProcess = uint.MaxValue;

    public int Invoke(string[] args)
    {
        var rootCommand = new RootCommand("WinGet Studio Command Line Interface")
        {
            new DscCommand(),
        };

        var parseResult = rootCommand.Parse(args);
        return parseResult.Invoke();
    }

    public void Initialize()
    {
        if (PInvoke.AttachConsole(AttachParentProcess))
        {
            PInvoke.AllocConsole();
        }

        try
        {
            _ = Console.Out;
            _ = Console.Error;
        }
        catch
        {
            // No-op
        }
    }
}

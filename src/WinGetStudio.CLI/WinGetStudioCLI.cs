// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Windows.Win32;
using WinGetStudio.CLI.DSCv3.Commands;
using WinGetStudio.Services.Logging.Extensions;
using WinGetStudio.Services.Settings.Contracts;
using WinGetStudio.Services.Settings.Extensions;

namespace WinGetStudio.CLI;

public sealed class WinGetStudioCLI
{
    private const uint AttachParentProcess = uint.MaxValue;

    public static IHost Host { get; private set; }

    public static IUserSettings UserSettings => Host.Services.GetService<IUserSettings>();

    public WinGetStudioCLI()
    {
        Initialize();
        Build();
    }

    public int Invoke(string[] args)
    {
        var rootCommand = new RootCommand("WinGet Studio Command Line Interface")
        {
            new DscCommand(),
        };

        var parseResult = rootCommand.Parse(args);
        return parseResult.Invoke();
    }

    private void Initialize()
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

    private void Build()
    {
        Host = Microsoft.Extensions.Hosting.Host
            .CreateDefaultBuilder()
            .UseContentRoot(AppContext.BaseDirectory)
            .UseDefaultServiceProvider((context, options) =>
            {
                options.ValidateOnBuild = true;
            })
            .ConfigureServices((context, services) =>
            {
                services.AddSettings();
                services.AddCustomLogging();
            })
            .Build();
    }
}

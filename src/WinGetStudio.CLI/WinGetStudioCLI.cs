// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Windows.Win32;
using WinGetStudio.Services.Logging.Extensions;
using WinGetStudio.Services.Settings.Contracts;
using WinGetStudio.Services.Settings.Extensions;

namespace WinGetStudio.CLI;

public sealed class WinGetStudioCLI
{
    private const uint AttachParentProcess = uint.MaxValue;
    private readonly WinGetStudioCommand _command;

    internal static IHost Host { get; private set; }

    internal static IUserSettings UserSettings => Host.Services.GetService<IUserSettings>();

    public WinGetStudioCLI()
    {
        _command = [];
        BuildHost();
        AttachConsole();
    }

    /// <summary>
    /// Invoke the CLI with the specified arguments.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    /// <returns> The exit code.</returns>
    public int Invoke(string[] args)
    {
        var parseResult = _command.Parse(args);
        return parseResult.Invoke();
    }

    /// <summary>
    /// Attach to the parent console if it exists, otherwise allocate a new
    /// console.
    /// </summary>
    private void AttachConsole()
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

    /// <summary>
    /// Build the host for dependency injection.
    /// </summary>
    private void BuildHost()
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

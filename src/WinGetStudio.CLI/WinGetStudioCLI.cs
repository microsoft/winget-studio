// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Windows.Win32;
using WinGetStudio.Services.Logging.Extensions;
using WinGetStudio.Services.Settings.Contracts;
using WinGetStudio.Services.Settings.Extensions;

namespace WinGetStudio.CLI;

public sealed class WinGetStudioCLI
{
    private const string AppSettingsFileName = "appsettings.cli.json";
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
        // Attach to the parent process console if possible; if that fails,
        // allocate a new console for this process.
        if (!PInvoke.AttachConsole(AttachParentProcess))
        {
            PInvoke.AllocConsole();
        }

        // Re-bind the managed Console output/error streams to the underlying
        // OS console handles so Console.Write/WriteLine and other APIs
        // write to the console we just attached/allocated.
        var stdout = Console.OpenStandardOutput();
        var stdoutWriter = new StreamWriter(stdout, Console.OutputEncoding) { AutoFlush = true };
        Console.SetOut(stdoutWriter);

        var stderr = Console.OpenStandardError();
        var stderrWriter = new StreamWriter(stderr, Console.OutputEncoding) { AutoFlush = true };
        Console.SetError(stderrWriter);
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
                services.AddLogging(AppSettingsFileName);
            })
            .Build();
    }
}

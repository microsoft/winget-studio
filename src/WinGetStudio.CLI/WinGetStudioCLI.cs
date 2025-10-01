// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Windows.Win32;
using WinGetStudio.CLI.Contracts;
using WinGetStudio.CLI.DSCv3.Extensions;
using WinGetStudio.CLI.Root.Commands;
using WinGetStudio.CLI.Root.Extensions;
using WinGetStudio.CLI.Services;
using WinGetStudio.CLI.Settings.Extensions;
using WingetStudio.Services.Localization.Extensions;
using WinGetStudio.Services.Logging.Extensions;
using WinGetStudio.Services.Settings.Extensions;

namespace WinGetStudio.CLI;

public sealed class WinGetStudioCLI
{
    private const string AppSettingsFileName = "appsettings.cli.json";
    private const uint AttachParentProcess = uint.MaxValue;
    private readonly IHost _host;

    public WinGetStudioCLI()
    {
        _host = BuildHost();
        AttachConsole();
    }

    /// <summary>
    /// Invoke the CLI with the specified arguments.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    /// <returns> The exit code.</returns>
    public int Invoke(string[] args)
    {
        var commandFactory = _host.Services.GetService<ICommandFactory>();
        var rootCommand = commandFactory.Create<WinGetStudioCommand>();
        var parseResult = rootCommand.Parse(args);
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
    private IHost BuildHost()
    {
        return Host
            .CreateDefaultBuilder()
            .UseContentRoot(AppContext.BaseDirectory)
            .UseDefaultServiceProvider((context, options) =>
            {
                options.ValidateOnBuild = true;
            })
            .ConfigureServices((context, services) =>
            {
                // Services
                services.AddSettings();
                services.AddLogging(AppSettingsFileName);
                services.AddReswLocalization();

                // Command flows
                services.AddRootCommandFlow();
                services.AddDscCommandFlow();
                services.AddSettingsCommandFlow();

                // Factories
                services.AddSingleton<ICommandFactory, CommandFactory>();
                services.AddSingleton<IOptionFactory, OptionFactory>();
            })
            .Build();
    }
}

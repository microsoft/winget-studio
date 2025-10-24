// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Services;

/// <summary>
/// This class is responsible for interacting with the DSC process to perform various operations.
/// </summary>
internal sealed partial class DSCProcess : IDSCProcess
{
    private readonly ILogger<DSCProcess> _logger;

    public DSCProcess(ILogger<DSCProcess> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task<DSCProcessResult> GetResourceSchemaAsync(string resource)
    {
        // This implementation can potentially be removed once this issue is resolved:
        // https://github.com/microsoft/winget-cli/issues/5829
        _logger.LogInformation($"Getting schema for DSC resource: {resource}");
        return Task.Run(() => ExecuteAsync("resource", "schema", "-r", resource, "-o", "json"));
    }

    /// <summary>
    /// Executes the DSC command with the specified arguments.
    /// </summary>
    /// <param name="args">The arguments to pass to the DSC command.</param>
    /// <returns>>The result of the DSC process execution.</returns>
    private async Task<DSCProcessResult> ExecuteAsync(params string[] args)
    {
        _logger.LogInformation($"Executing DSC command with arguments: dsc {string.Join(' ', args)}");
        var startInfo = new ProcessStartInfo
        {
            FileName = "dsc.exe",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        // Add arguments
        foreach (var arg in args)
        {
            startInfo.ArgumentList.Add(arg);
        }

        // Start the process
        using var process = new Process { StartInfo = startInfo };
        process.Start();

        // Read output and errors (if any)
        var output = process.StandardOutput.ReadToEnd();
        var errors = process.StandardError.ReadToEnd();

        // Wait for process to exit
        await process.WaitForExitAsync();

        // Log output and errors
        if (!string.IsNullOrEmpty(output))
        {
            _logger.LogInformation($"DSC Processor Output: {output}");
        }

        if (!string.IsNullOrEmpty(errors))
        {
            _logger.LogError($"DSC Processor Error: {errors}");
        }

        return new(output, errors, process.ExitCode);
    }
}

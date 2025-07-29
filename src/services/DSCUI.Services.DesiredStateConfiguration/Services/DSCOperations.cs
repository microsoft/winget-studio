// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using DSCUI.Services.DesiredStateConfiguration.Contracts;
using DSCUI.Services.DesiredStateConfiguration.Exceptions;
using DSCUI.Services.DesiredStateConfiguration.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Management.Configuration;
using Windows.Foundation;
using Windows.Storage.Streams;

namespace DSCUI.Services.DesiredStateConfiguration.Services;

internal sealed class DSCOperations : IDSCOperations
{
    private readonly ILogger _logger;
    private const string PowerShellHandlerIdentifier = "pwsh";

    public DSCOperations(ILogger<DSCOperations> logger)
    {
        _logger = logger;
    }

    public async Task<IDSCSet> OpenConfigurationSetAsync(IDSCFile file)
    {
        var processor = await CreateConfigurationProcessorAsync();
        var outOfProcResult = await OpenConfigurationSetAsync(file, processor);
        return new DSCSet(processor, outOfProcResult);
    }

    /// <inheritdoc />
    public IAsyncOperationWithProgress<IDSCApplySetResult, IDSCSetChangeData> ApplySetAsync(IDSCSet inputSet)
    {
        if (inputSet is not DSCSet dscSet)
        {
            throw new ArgumentException($"{nameof(inputSet)} must be of type {nameof(DSCSet)}", nameof(inputSet));
        }

        return AsyncInfo.Run<IDSCApplySetResult, IDSCSetChangeData>(async (cancellationToken, progress) => {
            _logger.LogInformation("Starting to apply configuration set");
            var task = dscSet.Processor.ApplySetAsync(dscSet.ConfigSet, ApplyConfigurationSetFlags.None);
            task.Progress += (sender, args) => progress.Report(new DSCSetChangeData(args));
            var outOfProcResult = await task;
            var inProcResult = new DSCApplySetResult(inputSet, outOfProcResult);
            _logger.LogInformation($"Apply configuration finished.");
            return inProcResult;
        });
    }

    /// <inheritdoc />
    public void GetConfigurationUnitDetails(IDSCSet inputSet)
    {
        if (inputSet is not DSCSet dscSet)
        {
            throw new ArgumentException($"{nameof(inputSet)} must be of type {nameof(DSCSet)}", nameof(inputSet));
        }

        _logger.LogInformation("Getting configuration unit details");
        var detailsOperation = dscSet.Processor.GetSetDetailsAsync(dscSet.ConfigSet, ConfigurationUnitDetailFlags.ReadOnly);
        var detailsOperationTask = detailsOperation.AsTask();

        // For each DSC unit, create a task to get the details asynchronously
        // in the background
        foreach (var unit in dscSet.UnitsInternal)
        {
            unit.SetLoadDetailsTask(Task.Run<IDSCUnitDetails>(async () =>
            {
                try
                {
                    await detailsOperationTask;
                    _logger.LogInformation($"Settings details for unit {unit.InstanceId}");
                    return GetCompleteUnitDetails(dscSet.ConfigSet, unit.InstanceId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to get details for unit {unit.InstanceId}");
                    return null;
                }
            }));
        }
    }

    /// <summary>
    /// Create a configuration processor using DSC configuration API
    /// </summary>
    /// <returns>Configuration processor</returns>
    private async Task<ConfigurationProcessor> CreateConfigurationProcessorAsync()
    {
        ConfigurationStaticFunctions config = new();
        var factory = await config.CreateConfigurationSetProcessorFactoryAsync(PowerShellHandlerIdentifier);

        // Create and configure the configuration processor.
        var processor = config.CreateConfigurationProcessor(factory);
        processor.Caller = nameof(DSCUI);
        processor.Diagnostics += LogConfigurationDiagnostics;
        processor.MinimumLevel = DiagnosticLevel.Verbose;
        return processor;
    }

    /// <summary>
    /// Open a configuration set using DSC configuration API
    /// </summary>
    /// <param name="file">Configuration file</param>
    /// <returns>Configuration set</returns>
    /// <exception cref="OpenConfigurationSetException">Thrown when the configuration set cannot be opened</exception>
    private async Task<ConfigurationSet> OpenConfigurationSetAsync(IDSCFile file, ConfigurationProcessor processor)
    {
        var inputStream = await StringToStreamAsync(file.Content);
        var openConfigResult = processor.OpenConfigurationSet(inputStream);
        var configSet = openConfigResult.Set ?? throw new OpenConfigurationSetException(openConfigResult.ResultCode, openConfigResult.Field, openConfigResult.Value);

        // Set input file path in the configuration set to inform the
        // processor about the working directory when applying the
        // configuration
        configSet.Name = file.Name;
        configSet.Origin = file.DirectoryPath;
        configSet.Path = file.Path;
        return configSet;
    }

    /// <summary>
    /// Map configuration diagnostics to logger
    /// </summary>
    /// <param name="sender">The event sender</param>
    /// <param name="diagnosticInformation">Diagnostic information</param>
    private void LogConfigurationDiagnostics(object sender, IDiagnosticInformation diagnosticInformation)
    {
        switch (diagnosticInformation.Level)
        {
            case DiagnosticLevel.Warning:
                _logger.LogWarning(diagnosticInformation.Message);
                return;
            case DiagnosticLevel.Error:
                _logger.LogError(diagnosticInformation.Message);
                return;
            case DiagnosticLevel.Critical:
                _logger.LogCritical(diagnosticInformation.Message);
                return;
            case DiagnosticLevel.Verbose:
                _logger.LogTrace(diagnosticInformation.Message);
                return;
            case DiagnosticLevel.Informational:
            default:
                _logger.LogInformation(diagnosticInformation.Message);
                return;
        }
    }

    /// <summary>
    /// Convert a string to an input stream
    /// </summary>
    /// <param name="str">Target string</param>
    /// <returns>Input stream</returns>
    private static async Task<InMemoryRandomAccessStream> StringToStreamAsync(string str)
    {
        InMemoryRandomAccessStream result = new();
        using (DataWriter writer = new(result))
        {
            writer.UnicodeEncoding = UnicodeEncoding.Utf8;
            writer.WriteString(str);
            await writer.StoreAsync();
            writer.DetachStream();
        }

        result.Seek(0);
        return result;
    }

    /// <summary>
    /// Gets the complete details for a unit if available.
    /// </summary>
    /// <param name="configSet">Configuration set</param>
    /// <param name="instanceId">Unit instance ID</param>
    /// <returns>Complete unit details if available, otherwise null</returns>
    private DSCUnitDetails GetCompleteUnitDetails(ConfigurationSet configSet, Guid instanceId)
    {
        var unitFound = configSet.Units.FirstOrDefault(u => u.InstanceIdentifier == instanceId);
        if (unitFound == null)
        {
            _logger.LogWarning($"Unit {instanceId} not found in the configuration set. No further details will be available to the unit.");
            return null;
        }

        if (unitFound.Details == null)
        {
            _logger.LogWarning($"Details for unit {instanceId} not found. No further details will be available to the unit.");
            return null;
        }

        // After GetSetDetailsAsync completes, the Details property will be
        // populated if the details were found.
        return new DSCUnitDetails(unitFound.Details);
    }
}

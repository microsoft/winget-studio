// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Management.Configuration;
using Windows.Foundation;
using Windows.Storage.Streams;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Exceptions;
using WinGetStudio.Services.DesiredStateConfiguration.Models;

namespace WinGetStudio.Services.DesiredStateConfiguration.Services;

internal sealed class DSCOperations : IDSCOperations
{
    private readonly ILogger _logger;
    private const string DSCv3DynamicRuntimeHandlerIdentifier = "{5f83e564-ca26-41ca-89db-36f5f0517ffd}";

    public DSCOperations(ILogger<DSCOperations> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<IDSCSet> OpenConfigurationSetAsync(IDSCFile file)
    {
        var processor = await CreateConfigurationProcessorAsync(DSCv3DynamicRuntimeHandlerIdentifier);
        var outOfProcResult = await OpenConfigurationSetAsync(file, processor);
        return new DSCSet(processor, outOfProcResult);
    }

    /// <inheritdoc />
    public IAsyncOperationWithProgress<IDSCApplySetResult, IDSCSetChangeData> ValidateSetAsync(IDSCSet inputSet)
    {
        if (inputSet is not DSCSet dscSet)
        {
            throw new ArgumentException($"{nameof(inputSet)} must be of type {nameof(DSCSet)}", nameof(inputSet));
        }

        return AsyncInfo.Run<IDSCApplySetResult, IDSCSetChangeData>(async (cancellationToken, progress) =>
        {
            _logger.LogInformation("Starting to validate configuration set");
            var inProcResult = await ApplySetInternalAsync(progress, dscSet, ApplyConfigurationSetFlags.PerformConsistencyCheckOnly);
            _logger.LogInformation($"Validate configuration finished.");
            return inProcResult;
        });
    }

    /// <inheritdoc />
    public IAsyncOperationWithProgress<IDSCApplySetResult, IDSCSetChangeData> ApplySetAsync(IDSCSet inputSet)
    {
        if (inputSet is not DSCSet dscSet)
        {
            throw new ArgumentException($"{nameof(inputSet)} must be of type {nameof(DSCSet)}", nameof(inputSet));
        }

        return AsyncInfo.Run<IDSCApplySetResult, IDSCSetChangeData>(async (cancellationToken, progress) =>
        {
            _logger.LogInformation("Starting to apply configuration set");
            var inProcResult = await ApplySetInternalAsync(progress, dscSet, ApplyConfigurationSetFlags.None);
            _logger.LogInformation($"Apply configuration finished.");
            return inProcResult;
        });
    }

    /// <inheritdoc/>
    public IAsyncOperationWithProgress<IDSCTestSetResult, IDSCTestUnitResult> TestSetAsync(IDSCSet inputSet)
    {
        if (inputSet is not DSCSet dscSet)
        {
            throw new ArgumentException($"{nameof(inputSet)} must be of type {nameof(DSCSet)}", nameof(inputSet));
        }

        return AsyncInfo.Run<IDSCTestSetResult, IDSCTestUnitResult>(async (cancellationToken, progress) =>
        {
            _logger.LogInformation("Starting to test configuration set");
            var task = dscSet.Processor.TestSetAsync(dscSet.ConfigSet);
            task.Progress += (sender, args) => progress.Report(new DSCTestUnitResult(args));
            var outOfProcResult = await task;
            var result = new DSCTestSetResult(outOfProcResult);
            _logger.LogInformation($"Test configuration finished with result: {result.TestResult}");
            return result;
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

    /// <inheritdoc />
    public async Task<IDSCGetUnitResult> GetUnitAsync(IDSCUnit inputUnit)
    {
        if (inputUnit is not DSCUnit dscUnit)
        {
            throw new ArgumentException($"{nameof(inputUnit)} must be of type {nameof(DSCUnit)}", nameof(inputUnit));
        }

        ConfigurationStaticFunctions config = new();
        var processor = await CreateConfigurationProcessorAsync(DSCv3DynamicRuntimeHandlerIdentifier);
        var result = await Task.Run(() => processor.GetUnitSettings(dscUnit.ConfigUnit));
        return new DSCGetUnitResult(result);
    }

    /// <inheritdoc />
    public async Task<IDSCApplyUnitResult> SetUnitAsync(IDSCUnit inputUnit)
    {
        if (inputUnit is not DSCUnit dscUnit)
        {
            throw new ArgumentException($"{nameof(inputUnit)} must be of type {nameof(DSCUnit)}", nameof(inputUnit));
        }

        ConfigurationStaticFunctions config = new();
        var processor = await CreateConfigurationProcessorAsync(DSCv3DynamicRuntimeHandlerIdentifier);
        var result = await Task.Run(() => processor.ApplyUnit(dscUnit.ConfigUnit));
        return new DSCApplyUnitResult(result);
    }

    /// <inheritdoc />
    public async Task<IDSCTestUnitResult> TestUnitAsync(IDSCUnit inputUnit)
    {
        if (inputUnit is not DSCUnit dscUnit)
        {
            throw new ArgumentException($"{nameof(inputUnit)} must be of type {nameof(DSCUnit)}", nameof(inputUnit));
        }

        ConfigurationStaticFunctions config = new();
        var processor = await CreateConfigurationProcessorAsync(DSCv3DynamicRuntimeHandlerIdentifier);
        var result = await Task.Run(() => processor.TestUnit(dscUnit.ConfigUnit));
        return new DSCTestUnitResult(result);
    }

    public async Task<IReadOnlyList<ResourceMetada>> GetDscV3ResourcesAsync()
    {
        List<ResourceMetada> resources = [];
        try
        {
            ConfigurationStaticFunctions config = new();
            var processor = await CreateConfigurationProcessorAsync(DSCv3DynamicRuntimeHandlerIdentifier);
            var options = config.CreateFindUnitProcessorsOptions();
            options.UnitDetailFlags = ConfigurationUnitDetailFlags.Local;

            // Retry a few times to workaround this issue:
            // https://github.com/PowerShell/DSC/issues/786
            // ---------------------------------------------
            // Find unit processors will call dsc.exe under the hood. This has
            // a known bug that makes it fail fairly often. To work around
            // this, we retry a few times. This is not a an ideal solution, but
            // it will allow us for now to get the resources most of the time.
            // Another downside of this approach is that if no resources are
            // actually present the code will always attempt 10 times before
            // giving up.
            var maxRetries = 10;
            IList<IConfigurationUnitProcessorDetails> units = [];
            while (units.Count == 0 && maxRetries > 0)
            {
                units = await processor.FindUnitProcessorsAsync(options);
                maxRetries--;
            }

            foreach (var unit in units)
            {
                resources.Add(new()
                {
                    Name = unit.UnitType,
                    Version = unit.Version,
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get DSC v3 resources");
        }

        return resources;
    }

    /// <summary>
    /// Create a configuration processor using DSC configuration API
    /// </summary>
    /// <returns>Configuration processor</returns>
    private async Task<ConfigurationProcessor> CreateConfigurationProcessorAsync(string handler)
    {
        ConfigurationStaticFunctions config = new();
        var factory = await config.CreateConfigurationSetProcessorFactoryAsync(handler);

        // Create and configure the configuration processor.
        var processor = config.CreateConfigurationProcessor(factory);
        processor.Caller = nameof(WinGetStudio);
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
        var configSet = openConfigResult.Set ?? throw new OpenConfigurationSetException(openConfigResult);

        // Set input file path in the configuration set to inform the
        // processor about the working directory when applying the
        // configuration
        if (file.FileInfo != null)
        {
            configSet.Name = file.FileInfo.Name;
            configSet.Origin = file.FileInfo.Directory.FullName;
            configSet.Path = file.FileInfo.FullName;
        }

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
                _logger.LogInformation(diagnosticInformation.Message);
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

    /// <summary>
    /// Applies a configuration set and reports progress.
    /// </summary>
    /// <param name="progress">Progress reporter</param>
    /// <param name="dscSet">Configuration set to apply</param>
    /// <param name="flags">Apply flags</param>
    /// <returns>Apply result</returns>
    private async Task<DSCApplySetResult> ApplySetInternalAsync(IProgress<IDSCSetChangeData> progress, DSCSet dscSet, ApplyConfigurationSetFlags flags)
    {
        var task = dscSet.Processor.ApplySetAsync(dscSet.ConfigSet, flags);
        task.Progress += (sender, args) => progress.Report(new DSCSetChangeData(args));
        var outOfProcResult = await task;
        var result = new DSCApplySetResult(dscSet, outOfProcResult);
        return result.IsOk ? result : throw new ApplyConfigurationSetException(result);
    }
}

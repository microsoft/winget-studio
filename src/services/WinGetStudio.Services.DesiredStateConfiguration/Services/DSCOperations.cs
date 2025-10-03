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
using WinGetStudio.Models;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Exceptions;
using WinGetStudio.Services.DesiredStateConfiguration.Models;

namespace WinGetStudio.Services.DesiredStateConfiguration.Services;

internal sealed class DSCOperations : IDSCOperations
{
    private readonly ILogger _logger;
    private readonly IDSCFactory _factory;

    public DSCOperations(IDSCFactory factory, ILogger<DSCOperations> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public async Task<IDSCSet> OpenConfigurationSetAsync(IDSCFile file)
    {
        var processor = await _factory.CreateProcessorAsync();
        var outOfProcResult = await OpenConfigurationSetAsync(file, processor);
        return new DSCSet(outOfProcResult);
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
            var processor = await _factory.CreateProcessorAsync();
            var task = processor.ApplySetAsync(dscSet.ConfigSet, ApplyConfigurationSetFlags.None);
            task.Progress += (sender, args) => progress.Report(new DSCSetChangeData(args));
            var outOfProcResult = await task;
            var inProcResult = new DSCApplySetResult(inputSet, outOfProcResult);
            _logger.LogInformation($"Apply configuration finished.");
            return inProcResult;
        });
    }

    /// <inheritdoc />
    public async Task GetConfigurationUnitDetailsAsync(IDSCSet inputSet)
    {
        if (inputSet is not DSCSet dscSet)
        {
            throw new ArgumentException($"{nameof(inputSet)} must be of type {nameof(DSCSet)}", nameof(inputSet));
        }

        _logger.LogInformation("Getting configuration unit details");
        var processor = await _factory.CreateProcessorAsync();
        var detailsOperation = processor.GetSetDetailsAsync(dscSet.ConfigSet, ConfigurationUnitDetailFlags.ReadOnly);
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
    public async Task GetUnit(ConfigurationUnitModel unit)
    {
        ConfigurationStaticFunctions config = new();
        var processor = await _factory.CreateProcessorAsync();
        var input = config.CreateConfigurationUnit();
        input.Settings = unit.Settings;
        input.Type = unit.Type;

        var result = await Task.Run(() => processor.GetUnitSettings(input));
        unit.Settings = result.Settings;
    }

    /// <inheritdoc />
    public async Task SetUnit(ConfigurationUnitModel unit)
    {
        ConfigurationStaticFunctions config = new();
        var processor = await _factory.CreateProcessorAsync();
        var input = config.CreateConfigurationUnit();
        input.Settings = unit.Settings;
        input.Type = unit.Type;

        var result = await Task.Run(() => processor.ApplyUnit(input));
        System.Diagnostics.Debug.WriteLine($"SetUnit result: {result.PreviouslyInDesiredState}");
    }

    /// <inheritdoc />
    public async Task TestUnit(ConfigurationUnitModel unit)
    {
        ConfigurationStaticFunctions config = new();
        var processor = await _factory.CreateProcessorAsync();
        var input = config.CreateConfigurationUnit();
        input.Settings = unit.Settings;
        input.Type = unit.Type;
        var result = await Task.Run(() => processor.TestUnit(input));
        System.Diagnostics.Debug.WriteLine($"TestUnit result: {result.TestResult}");
        unit.TestResult = result.TestResult == ConfigurationTestResult.Positive;
    }

    /// <inheritdoc />
    /// Currently broken due to bug in DSC
    /// https://github.com/PowerShell/DSC/issues/786
    public async Task ExportUnit(ConfigurationUnitModel unit)
    {
        ConfigurationStaticFunctions config = new();
        var processor = await _factory.CreateProcessorAsync();
        var input = config.CreateConfigurationUnit();
        input.Type = unit.Type;
        input.Intent = ConfigurationUnitIntent.Inform;

        var result = processor.GetAllUnits(input);

        if (result.Units != null)
        {
            System.Diagnostics.Debug.WriteLine(result.Units);
            unit.Settings = result.Units[0].Settings;
        }
    }

    public async Task<IReadOnlyList<ResourceMetadata>> GetDscV3ResourcesAsync()
    {
        List<ResourceMetadata> resources = [];
        try
        {
            ConfigurationStaticFunctions config = new();
            var processor = await _factory.CreateProcessorAsync();
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

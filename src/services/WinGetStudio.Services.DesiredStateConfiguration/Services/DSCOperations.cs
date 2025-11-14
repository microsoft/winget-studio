// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Management.Configuration;
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
    public async Task<IDSCSet> OpenConfigurationSetAsync(IDSCFile file, CancellationToken ct = default)
    {
        var processor = await CreateConfigurationProcessorAsync(DSCv3DynamicRuntimeHandlerIdentifier);
        var outOfProcResult = await OpenConfigurationSetInternalAsync(file, processor, ct);
        return new DSCSet(processor, outOfProcResult);
    }

    /// <inheritdoc />
    public async Task<IDSCApplySetResult> ValidateSetAsync(IDSCSet inputSet, IProgress<IDSCSetChangeData> progress = null, CancellationToken ct = default)
    {
        if (inputSet is not DSCSet dscSet)
        {
            throw new ArgumentException($"{nameof(inputSet)} must be of type {nameof(DSCSet)}", nameof(inputSet));
        }

        _logger.LogInformation("Starting to validate configuration set");
        var inProcResult = await ApplySetInternalAsync(progress, dscSet, ApplyConfigurationSetFlags.PerformConsistencyCheckOnly, ct).ConfigureAwait(false);
        _logger.LogInformation($"Validate configuration finished.");
        return inProcResult;
    }

    /// <inheritdoc />
    public async Task<IDSCApplySetResult> ApplySetAsync(IDSCSet inputSet, IProgress<IDSCSetChangeData> progress = null, CancellationToken ct = default)
    {
        if (inputSet is not DSCSet dscSet)
        {
            throw new ArgumentException($"{nameof(inputSet)} must be of type {nameof(DSCSet)}", nameof(inputSet));
        }

        _logger.LogInformation("Starting to apply configuration set");
        var inProcResult = await ApplySetInternalAsync(progress, dscSet, ApplyConfigurationSetFlags.None, ct).ConfigureAwait(false);
        _logger.LogInformation($"Apply configuration finished.");
        return inProcResult;
    }

    /// <inheritdoc/>
    public async Task<IDSCTestSetResult> TestSetAsync(IDSCSet inputSet, IProgress<IDSCTestUnitResult> progress = null, CancellationToken ct = default)
    {
        if (inputSet is not DSCSet dscSet)
        {
            throw new ArgumentException($"{nameof(inputSet)} must be of type {nameof(DSCSet)}", nameof(inputSet));
        }

        _logger.LogInformation("Starting to test configuration set");
        ct.ThrowIfCancellationRequested();
        var task = dscSet.Processor.TestSetAsync(dscSet.ConfigSet);
        task.Progress += (sender, args) => progress?.Report(new DSCTestUnitResult(args));
        using var reg = ct.Register(task.Cancel);
        var outOfProcResult = await task.AsTask(ct).ConfigureAwait(false);
        var result = new DSCTestSetResult(outOfProcResult);
        _logger.LogInformation($"Test configuration finished with result: {result.TestResult}");
        return result;
    }

    /// <inheritdoc />
    public async Task<IDSCGetSetDetailsResult> GetSetDetailsAsync(IDSCSet inputSet, IProgress<IDSCGetUnitDetailsResult> progress = null, CancellationToken ct = default)
    {
        if (inputSet is not DSCSet dscSet)
        {
            throw new ArgumentException($"{nameof(inputSet)} must be of type {nameof(DSCSet)}", nameof(inputSet));
        }

        _logger.LogInformation("Getting configuration set details");
        ct.ThrowIfCancellationRequested();
        var task = dscSet.Processor.GetSetDetailsAsync(dscSet.ConfigSet, ConfigurationUnitDetailFlags.ReadOnly);
        task.Progress += (sender, args) => progress?.Report(new DSCGetUnitDetailsResult(args));
        using var reg = ct.Register(task.Cancel);
        var outOfProcResult = await task.AsTask(ct).ConfigureAwait(false);
        var result = new DSCGetSetDetailsResult(outOfProcResult);
        _logger.LogInformation("Get configuration set details finished");
        return result;
    }

    /// <inheritdoc />
    public async Task<IDSCGetUnitDetailsResult> GetUnitDetailsAsync(IDSCUnit inputUnit, CancellationToken ct = default)
    {
        if (inputUnit is not DSCUnit dscUnit)
        {
            throw new ArgumentException($"{nameof(inputUnit)} must be of type {nameof(DSCUnit)}", nameof(inputUnit));
        }

        _logger.LogInformation("Getting configuration unit details");
        ct.ThrowIfCancellationRequested();
        var processor = await CreateConfigurationProcessorAsync(DSCv3DynamicRuntimeHandlerIdentifier);
        var task = processor.GetUnitDetailsAsync(dscUnit.ConfigUnit, ConfigurationUnitDetailFlags.ReadOnly);
        using var reg = ct.Register(task.Cancel);
        var outOfProcResult = await task.AsTask(ct).ConfigureAwait(false);
        var result = new DSCGetUnitDetailsResult(outOfProcResult);
        _logger.LogInformation("Get configuration unit details finished");
        return result;
    }

    /// <inheritdoc />
    public async Task<IDSCGetUnitResult> GetUnitAsync(IDSCUnit inputUnit, CancellationToken ct = default)
    {
        if (inputUnit is not DSCUnit dscUnit)
        {
            throw new ArgumentException($"{nameof(inputUnit)} must be of type {nameof(DSCUnit)}", nameof(inputUnit));
        }

        _logger.LogInformation($"Getting unit settings for unit with ModuleName={inputUnit.ModuleName}, Type={inputUnit.Type}");
        ct.ThrowIfCancellationRequested();
        var processor = await CreateConfigurationProcessorAsync(DSCv3DynamicRuntimeHandlerIdentifier);
        var task = processor.GetUnitSettingsAsync(dscUnit.ConfigUnit);
        using var reg = ct.Register(task.Cancel);
        var result = await task.AsTask(ct).ConfigureAwait(false);
        return new DSCGetUnitResult(result);
    }

    /// <inheritdoc />
    public async Task<IDSCApplyUnitResult> SetUnitAsync(IDSCUnit inputUnit, CancellationToken ct = default)
    {
        if (inputUnit is not DSCUnit dscUnit)
        {
            throw new ArgumentException($"{nameof(inputUnit)} must be of type {nameof(DSCUnit)}", nameof(inputUnit));
        }

        _logger.LogInformation($"Setting unit settings for unit with ModuleName={inputUnit.ModuleName}, Type={inputUnit.Type}");
        ct.ThrowIfCancellationRequested();
        var processor = await CreateConfigurationProcessorAsync(DSCv3DynamicRuntimeHandlerIdentifier);
        var task = processor.ApplyUnitAsync(dscUnit.ConfigUnit);
        using var reg = ct.Register(task.Cancel);
        var result = await task.AsTask(ct).ConfigureAwait(false);
        return new DSCApplyUnitResult(result);
    }

    /// <inheritdoc />
    public async Task<IDSCTestUnitResult> TestUnitAsync(IDSCUnit inputUnit, CancellationToken ct = default)
    {
        if (inputUnit is not DSCUnit dscUnit)
        {
            throw new ArgumentException($"{nameof(inputUnit)} must be of type {nameof(DSCUnit)}", nameof(inputUnit));
        }

        _logger.LogInformation($"Testing unit settings for unit with ModuleName={inputUnit.ModuleName}, Type={inputUnit.Type}");
        ct.ThrowIfCancellationRequested();
        var processor = await CreateConfigurationProcessorAsync(DSCv3DynamicRuntimeHandlerIdentifier);
        var task = processor.TestUnitAsync(dscUnit.ConfigUnit);
        using var reg = ct.Register(task.Cancel);
        var result = await task.AsTask(ct).ConfigureAwait(false);
        return new DSCTestUnitResult(result);
    }

    public async Task<IReadOnlyList<ResourceMetadata>> GetDscV3ResourcesAsync()
    {
        List<ResourceMetadata> resources = [];
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
            // this, we retry a few times. This is not an ideal solution, but
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
    /// <param name="ct">Cancellation token</param>
    /// <returns>Configuration set</returns>
    /// <exception cref="OpenConfigurationSetException">Thrown when the configuration set cannot be opened</exception>
    private async Task<ConfigurationSet> OpenConfigurationSetInternalAsync(IDSCFile file, ConfigurationProcessor processor, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var inputStream = await StringToStreamAsync(file.Content);
        var task = processor.OpenConfigurationSetAsync(inputStream);
        using var reg = ct.Register(task.Cancel);
        var result = await task.AsTask(ct).ConfigureAwait(false);
        var configSet = result.Set ?? throw new OpenConfigurationSetException(result);

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
    /// Applies a configuration set and reports progress.
    /// </summary>
    /// <param name="progress">Progress reporter</param>
    /// <param name="dscSet">Configuration set to apply</param>
    /// <param name="flags">Apply flags</param>
    /// <returns>Apply result</returns>
    private async Task<DSCApplySetResult> ApplySetInternalAsync(
        IProgress<IDSCSetChangeData> progress,
        DSCSet dscSet,
        ApplyConfigurationSetFlags flags,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var task = dscSet.Processor.ApplySetAsync(dscSet.ConfigSet, flags);
        task.Progress += (sender, args) => progress?.Report(new DSCSetChangeData(args));
        using var reg = ct.Register(task.Cancel);
        var outOfProcResult = await task.AsTask(ct).ConfigureAwait(false);
        var result = new DSCApplySetResult(dscSet, outOfProcResult);
        return result.IsOk ? result : throw new ApplyConfigurationSetException(result);
    }
}

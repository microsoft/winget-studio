// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Management.Configuration;
using Windows.Foundation.Collections;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Models;

namespace WinGetStudio.Services.DesiredStateConfiguration.Services;

internal sealed class DSCFactory : IDSCFactory
{
    private readonly ILogger<DSCFactory> _logger;
    private const string DSCv3DynamicRuntimeHandlerIdentifier = "{5f83e564-ca26-41ca-89db-36f5f0517ffd}";

    public DSCFactory(ILogger<DSCFactory> logger)
    {
        _logger = logger;
    }

    public IDSCSet CreateSet(IDSCSet set)
    {
        if (set is EditableDSCSet editableDSCSet)
        {
            ConfigurationStaticFunctions config = new();
            var configSet = config.CreateConfigurationSet();
            configSet.Name = editableDSCSet.Name;
            foreach (var unit in editableDSCSet.Units)
            {
                var configUnit = CreateConfigurationUnit(unit);
                configSet.Units.Add(configUnit);
            }

            return new DSCSet(configSet);
        }

        return set;
    }

    public IDSCUnit CreateUnit(IDSCUnit unit)
    {
        if (unit is EditableDSCUnit editableUnit)
        {
            var configUnit = CreateConfigurationUnit(editableUnit);
            return new DSCUnit(configUnit);
        }

        return unit;
    }

    private void ConvertKeyValuePairListToValueSet(ValueSet valueSet, IList<KeyValuePair<string, object>> settings)
    {
        foreach (var setting in settings)
        {
            if (setting.Value is List<KeyValuePair<string, object>> kvpList)
            {
                ValueSet nestedValueSet = new();
                ConvertKeyValuePairListToValueSet(nestedValueSet, kvpList);
                valueSet.Add(setting.Key, nestedValueSet);
            }
            else
            {
                valueSet.Add(setting.Key, setting.Value);
            }
        }
    }

    /// <inheritdoc/>
    public async Task<ConfigurationProcessor> CreateProcessorAsync()
    {
        ConfigurationStaticFunctions config = new();
        var factory = await config.CreateConfigurationSetProcessorFactoryAsync(DSCv3DynamicRuntimeHandlerIdentifier);

        // Create and configure the configuration processor.
        var processor = config.CreateConfigurationProcessor(factory);
        processor.Caller = nameof(WinGetStudio);
        processor.Diagnostics += LogConfigurationDiagnostics;
        processor.MinimumLevel = DiagnosticLevel.Verbose;
        return processor;
    }

    private ConfigurationUnit CreateConfigurationUnit(IDSCUnit unit)
    {
        ConfigurationStaticFunctions config = new();
        var configUnit = config.CreateConfigurationUnit();
        configUnit.Type = unit.Type;
        configUnit.Identifier = unit.Id;

        configUnit.Intent = unit.Intent;
        configUnit.Environment.Context = unit.SecurityContext;

        configUnit.Metadata = new ValueSet();
        foreach (var metadata in unit.Metadata)
        {
            configUnit.Metadata.Add(metadata.Key, metadata.Value);
        }

        if (unit.ModuleName != string.Empty)
        {
            configUnit.Metadata["module"] = unit.ModuleName;
        }

        if (unit.Description != string.Empty)
        {
            configUnit.Metadata["description"] = unit.Description;
        }

        configUnit.Settings.Clear();
        ConvertKeyValuePairListToValueSet(configUnit.Settings, unit.Settings);

        configUnit.Dependencies.Clear();
        foreach (var dependency in unit.Dependencies)
        {
            configUnit.Dependencies.Add(dependency);
        }

        return configUnit;
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
}

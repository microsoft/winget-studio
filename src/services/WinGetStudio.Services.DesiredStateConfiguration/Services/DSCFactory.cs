// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Models;
using Microsoft.Management.Configuration;
using Windows.Foundation.Collections;

namespace WinGetStudio.Services.DesiredStateConfiguration.Services;
internal class DSCFactory : IDSCFactory
{
    private ConfigurationProcessor _configurationProcessor;
    private const string DSCv3DynamicRuntimeHandlerIdentifier = "{5f83e564-ca26-41ca-89db-36f5f0517ffd}";

    public DSCFactory()
    {
        _configurationProcessor = null;
    }

    public async Task<IDSCSet> CreateSetAsync(IDSCSet set)
    {
        if(_configurationProcessor == null)
        {
            await CreateProcessorAsync();
        }

        if (set is EditableDSCSet editableDSCSet)
        {
            ConfigurationStaticFunctions config = new();
            var configSet = config.CreateConfigurationSet();
            configSet.Name = editableDSCSet.Name;
            DSCSet newDSCSet = new(_configurationProcessor, configSet);
            foreach (var unit in editableDSCSet.InternalUnits)
            {
                var u = CreateUnit(unit);
                newDSCSet.UnitsInternal.Add(u as DSCUnit);
                newDSCSet.ConfigSet.Units.Add((u as DSCUnit).ConfigUnit);
            }
            return newDSCSet;
        }
        return set;
    }

    public IDSCUnit CreateUnit(IDSCUnit unit)
    {
        if (unit is DSCUnit u)
        {
            return u;
        }
        else if (unit is EditableDSCUnit editableUnit)
        {
            ConfigurationStaticFunctions config = new();
            var configUnit = config.CreateConfigurationUnit();
            configUnit.Type = editableUnit.Type;
            configUnit.Identifier = editableUnit.Id;

            configUnit.Intent = Enum.Parse<ConfigurationUnitIntent>(editableUnit.Intent);
            configUnit.Environment.Context = editableUnit.RequiresElevation ? SecurityContext.Elevated : SecurityContext.Current;

            configUnit.Metadata = new ValueSet();
            foreach (var metadata in editableUnit.Metadata)
            {
                configUnit.Metadata.Add(metadata.Key, metadata.Value);
            }
            if (editableUnit.ModuleName != string.Empty)
            {
                configUnit.Metadata["module"] = editableUnit.ModuleName;
            }
            if (editableUnit.Description != string.Empty)
            {
                configUnit.Metadata["description"] = editableUnit.Description;
            }

            configUnit.Settings.Clear();
            ConvertKeyValuePairListToValueSet(configUnit.Settings, editableUnit.Settings);

            configUnit.Dependencies.Clear();
            foreach (var dependency in editableUnit.Dependencies)
            {
                configUnit.Dependencies.Add(dependency);
            }
            DSCUnit dscUnit = new(configUnit);
            return dscUnit;
        }
        else
        {
            throw new ArgumentException("Unsupported unit type", nameof(unit));
        }
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

    private async Task CreateProcessorAsync()
    {
        ConfigurationStaticFunctions config = new();
        var factory = await config.CreateConfigurationSetProcessorFactoryAsync(DSCv3DynamicRuntimeHandlerIdentifier);

        // Create and configure the configuration processor.
        _configurationProcessor = config.CreateConfigurationProcessor(factory);
        _configurationProcessor.Caller = nameof(WinGetStudio);
        _configurationProcessor.MinimumLevel = DiagnosticLevel.Verbose;
    }
}

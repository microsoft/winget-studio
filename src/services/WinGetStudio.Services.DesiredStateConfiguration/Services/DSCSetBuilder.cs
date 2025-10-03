// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Management.Configuration;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace WinGetStudio.Services.DesiredStateConfiguration.Services;

internal sealed class DSCSetBuilder : IDSCSetBuilder
{
    private readonly IDSCFactory _dscFactory;
    private readonly ILogger _logger;
    private EditableDSCSet _dscSet = new();
    private const string DscSchemaUrl = "https://raw.githubusercontent.com/PowerShell/DSC/main/schemas/2023/08/config/document.json";
    private const string DscVersionString = "dscv3";

    public IReadOnlyList<IDSCUnit> Units => _dscSet.Units;

    public string TargetFilePath { get; set; } = string.Empty;

    public DSCSetBuilder(IDSCFactory dscFactory, ILogger<DSCSetBuilder> logger)
    {
        _dscFactory = dscFactory;
        _logger = logger;
    }

    public void AddUnit(EditableDSCUnit unit)
    {
        _dscSet.AddUnit(unit);
    }

    public IDSCSet Build()
    {
        return _dscFactory.CreateSet(_dscSet);
    }

    public void ClearUnits()
    {
        _dscSet.ClearUnits();
    }

    public void RemoveUnit(EditableDSCUnit unit)
    {
        _dscSet.RemoveUnit(unit);
    }

    public void UpdateUnit(EditableDSCUnit unit)
    {
        _dscSet.UpdateUnit(unit);
    }

    public void ImportSet(IDSCSet set)
    {
        if (set is EditableDSCSet e)
        {
            _dscSet = e;
        }
        else if (set is DSCSet dscSet)
        {
            _dscSet = new();
            _dscSet.Name = dscSet.Name;

            foreach (var unit in dscSet.UnitsInternal)
            {
                var u = new EditableDSCUnit(unit);
                _dscSet.AddUnit(u);
            }
        }
    }

    public bool IsEmpty() => Units.Count == 0;

    public string ConvertToYaml()
    {
        var dscSet = Build();
        var dscYaml = new Dictionary<string, object>
        {
            ["$schema"] = DscSchemaUrl,
            ["metadata"] = new Dictionary<string, object>
            {
                ["winget"] = new Dictionary<string, object>
                {
                    ["processor"] = DscVersionString,
                },
            },
            ["resources"] = new List<Dictionary<string, object>>(),
        };

        foreach (var unit in dscSet.Units)
        {
            var metadata = unit.Metadata.ToDictionary(kv => kv.Key, kv => kv.Value);
            if (unit.SecurityContext == SecurityContext.Elevated)
            {
                metadata.Add("securityContext", "elevated");
            }

            (dscYaml["resources"] as List<Dictionary<string, object>>).Add(new Dictionary<string, object>
            {
                ["name"] = unit.Id,
                ["type"] = unit.Type,
                ["metadata"] = metadata,
                ["properties"] = unit.Settings.ToDictionary(kv => kv.Key, kv => kv.Value),
            });
        }

        var serializer = new SerializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .WithIndentedSequences()
        .WithMaximumRecursion(100)
        .Build();
        try
        {
            var yaml = serializer.Serialize(dscYaml);
            return yaml;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Failed to serialize configuration set to YAML. Error: {e.Message}");
            return string.Empty;
        }
    }

    public bool EqualsYaml(string yaml)
    {
        return ConvertToYaml() == yaml;
    }
}

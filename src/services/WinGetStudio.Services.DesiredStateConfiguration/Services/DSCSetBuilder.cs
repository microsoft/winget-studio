// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace WinGetStudio.Services.DesiredStateConfiguration.Services;

internal sealed class DSCSetBuilder : IDSCSetBuilder
{
    private readonly IDSCFactory _dscFactory;
    private EditableDSCSet _dscSet = new();
    private const string DscSchemaUrl = "https://raw.githubusercontent.com/PowerShell/DSC/main/schemas/2023/08/config/document.json";
    private const string DscVersionSrting = "dscv3";

    public IReadOnlyList<IDSCUnit> Units => _dscSet.Units;

    public string TargetFilePath { get; set; } = string.Empty;

    public DSCSetBuilder(IDSCFactory dscFactory)
    {
        _dscFactory = dscFactory;
    }

    public void AddUnit(EditableDSCUnit unit)
    {
        _dscSet.AddUnit(unit);
    }

    public async Task<IDSCSet> BuildAsync()
    {
        return await _dscFactory.CreateSetAsync(_dscSet);
    }

    public void ClearUnits()
    {
        _dscSet.InternalUnits.Clear();
    }

    public void RemoveUnit(EditableDSCUnit unit)
    {
        _dscSet.InternalUnits.Remove(unit);
    }

    public void UpdateUnit(EditableDSCUnit unit)
    {
        var existingUnit = _dscSet.InternalUnits.FirstOrDefault(u => u.InstanceId == unit.InstanceId);
        if (existingUnit != null)
        {
            var i = _dscSet.InternalUnits.FindIndex(u => u.InstanceId == unit.InstanceId);
            _dscSet.InternalUnits[i] = unit;
        }
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
                _dscSet.InternalUnits.Add(u);
            }
        }
    }

    public bool IsEmpty() => Units.Count == 0;

    public async Task<string> ConvertToYamlAsync()
    {
        var dscSet = await BuildAsync();
        var dscYaml = new Dictionary<string, object>
        {
            ["$schema"] = DscSchemaUrl,
            ["metadata"] = new Dictionary<string, object>
            {
                ["winget"] = new Dictionary<string, object>
                {
                    ["processor"] = DscVersionSrting,
                },
            },
            ["resources"] = new List<Dictionary<string, object>>(),
        };

        foreach (var unit in dscSet.Units)
        {
            var metadata = unit.Metadata.ToDictionary(kv => kv.Key, kv => kv.Value);
            if (unit.RequiresElevation)
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
        .Build();

        var yaml = serializer.Serialize(dscYaml);
        return yaml;
    }

    public async Task<bool> EqualsYamlAsync(string yaml)
    {
        return await ConvertToYamlAsync() == yaml;
    }
}

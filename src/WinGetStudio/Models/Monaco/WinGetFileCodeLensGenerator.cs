// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Extensions.Localization;
using WinGetStudio.Views.Controls;
using YamlDotNet.RepresentationModel;

namespace WinGetStudio.Models.Monaco;

public sealed partial class WinGetFileCodeLensGenerator
{
    private readonly IStringLocalizer _localizer;

    public const string EditResourceCommandId = "editResourceCommand";
    public const string ValidateUnitCommandId = "validateUnitCommand";

    public WinGetFileCodeLensGenerator(IStringLocalizer localizer)
    {
        _localizer = localizer;
    }

    public bool TryGenerateCodeLenses(string? code, out string codeLenses)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            codeLenses = string.Empty;
            return false;
        }

        try
        {
            // Load the YAML
            var yaml = new YamlStream();
            yaml.Load(new StringReader(code));
            var root = (YamlMappingNode)yaml.Documents[0].RootNode;

            // Handle resources
            var resourcesNode = (YamlSequenceNode)root.Children[new YamlScalarNode("resources")];
            var codeLensesList = GenerateResourceCodeLenses(resourcesNode);
            codeLenses = JsonSerializer.Serialize(codeLensesList);
            return true;
        }
        catch
        {
            codeLenses = string.Empty;
            return false;
        }
    }

    private List<MonacoEditor.MonacoCodeLens<int>> GenerateResourceCodeLenses(YamlSequenceNode resourcesNode)
    {
        List<MonacoEditor.MonacoCodeLens<int>> codeLenses = [];
        for (var i = 0; i < resourcesNode.Children.Count; i++)
        {
            var resource = (YamlMappingNode)resourcesNode.Children[i];
            var start = resource.Start;
            var end = resource.End;
            var range = new MonacoEditor.MonacoRange(start.Line, start.Column, end.Line, end.Column);

            // Add all code lenses for this resource
            codeLenses.Add(CreateEditResourceCodeLens(range, i));
            codeLenses.Add(CreateValidateUnitCodeLens(range, i));
        }

        return codeLenses;
    }

    private MonacoEditor.MonacoCodeLens<int> CreateValidateUnitCodeLens(MonacoEditor.MonacoRange range, int index)
    {
        return new(range)
        {
            Command = new("codeLensCommand", _localizer["MonacoEditor_ValidateText"])
            {
                Tooltip = _localizer["MonacoEditor_ValidateTextTooltip"],
                Arguments = [new() { Id = ValidateUnitCommandId, Value = index }],
            },
        };
    }

    private MonacoEditor.MonacoCodeLens<int> CreateEditResourceCodeLens(MonacoEditor.MonacoRange range, int index)
    {
        return new(range)
        {
            Command = new("codeLensCommand", _localizer["MonacoEditor_EditText"])
            {
                Tooltip = _localizer["MonacoEditor_EditTextTooltip"],
                Arguments = [new() { Id = EditResourceCommandId, Value = index }],
            },
        };
    }
}

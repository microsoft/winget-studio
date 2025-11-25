// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Localization;
using YamlDotNet.RepresentationModel;
using static WinGetStudio.Views.Controls.MonacoEditor;

namespace WinGetStudio.Helpers;

public static partial class WinGetFileCodeLensHelper
{
    public const string EditResourceCommandId = "editResourceCommand";
    public const string ValidateUnitCommandId = "validateUnitCommand";

    private const string PropertiesKey = "properties";
    private const string ResourcesKey = "resources";
    private const string AssertionsKey = "assertions";

    /// <summary>
    /// Try to generate code lenses for WinGet files.
    /// </summary>
    /// <param name="localizer">The localizer.</param>
    /// <param name="code">The YAML code.</param>
    /// <param name="codeLenses">The generated code lenses.</param>
    /// <returns>True if code lenses were generated; otherwise, false.</returns>
    public static bool TryGenerateCodeLenses(IStringLocalizer localizer, string? code, out List<MonacoCodeLens> codeLenses)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            codeLenses = [];
            return false;
        }

        try
        {
            var root = GetRootNode(code);

            // Try to generate code for older formats first: 0.1.0 and 0.2.0
            if (TryGenerateCodeLensesV1AndV2(localizer, root, out codeLenses))
            {
                return true;
            }

            // Try to generate code for newer dsc schemas
            if (TryGenerateCodeLensesV3(localizer, root, out codeLenses))
            {
                return true;
            }

            return false;
        }
        catch
        {
            codeLenses = [];
            return false;
        }
    }

    /// <summary>
    /// Try to generate code lenses for WinGet files.
    /// </summary>
    /// <remarks>
    /// Example of a winget file supported by this method:
    /// <code>
    /// properties:
    ///   assertions:
    ///     - resource: Module/Resource
    ///   resources:
    ///     - resource: Module/Resource
    /// </code>
    /// </remarks>
    /// <param name="localizer">The localizer.</param>
    /// <param name="root">The root YAML mapping node.</param>
    /// <param name="codeLenses">The generated code lenses.</param>
    /// <returns>True if code lenses were generated; otherwise, false.</returns>
    private static bool TryGenerateCodeLensesV1AndV2(IStringLocalizer localizer, YamlMappingNode root, out List<MonacoCodeLens> codeLenses)
    {
        codeLenses = [];

        try
        {
            if (root.Children.TryGetValue(new YamlScalarNode(PropertiesKey), out var propertiesNode) && propertiesNode is YamlMappingNode properties)
            {
                var offsetIndex = 0;

                // First check for assertions
                if (properties.Children.TryGetValue(new YamlScalarNode(AssertionsKey), out var assertionsNode) && assertionsNode is YamlSequenceNode assertions)
                {
                    codeLenses.AddRange(GenerateResourceCodeLenses(localizer, assertions, offsetIndex));
                    offsetIndex += assertions.Children.Count;
                }

                // Second check for resources
                if (properties.Children.TryGetValue(new YamlScalarNode(ResourcesKey), out var resourcesNode) && resourcesNode is YamlSequenceNode resources)
                {
                    codeLenses.AddRange(GenerateResourceCodeLenses(localizer, resources, offsetIndex));
                }

                return true;
            }

            // Format not supported
            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Try to generate code lenses for WinGet files.
    /// </summary>
    /// <remarks>
    /// Example of a winget file supported by this method:
    /// <code>
    /// resources:
    ///   - type: Module/Resource
    /// </code>
    /// </remarks>
    /// <param name="localizer">The localizer.</param>
    /// <param name="root">The root YAML mapping node.</param>
    /// <param name="codeLenses">The generated code lenses.</param>
    /// <returns>True if code lenses were generated; otherwise, false.</returns>
    private static bool TryGenerateCodeLensesV3(IStringLocalizer localizer, YamlMappingNode root, out List<MonacoCodeLens> codeLenses)
    {
        codeLenses = [];

        try
        {
            if (root.Children.TryGetValue(new YamlScalarNode(ResourcesKey), out var resourcesNode) && resourcesNode is YamlSequenceNode resources)
            {
                codeLenses.AddRange(GenerateResourceCodeLenses(localizer, resources));
                return true;
            }

            // Format not supported
            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Get the root YAML mapping node from the given code.
    /// </summary>
    /// <param name="code">The YAML code.</param>
    /// <returns>The root YAML mapping node.</returns>
    private static YamlMappingNode GetRootNode(string code)
    {
        var yaml = new YamlStream();
        yaml.Load(new StringReader(code));
        return (YamlMappingNode)yaml.Documents[0].RootNode;
    }

    /// <summary>
    /// Generate code lenses for the given resources.
    /// </summary>
    /// <param name="localizer">The localizer.</param>
    /// <param name="resources">The YAML sequence node containing the resources.</param>
    /// <returns>A list of generated code lenses.</returns>
    private static List<MonacoCodeLens> GenerateResourceCodeLenses(IStringLocalizer localizer, YamlSequenceNode resources, int indexOffset = 0)
    {
        List<MonacoCodeLens> codeLenses = [];
        for (var i = 0; i < resources.Children.Count; i++)
        {
            var resource = (YamlMappingNode)resources.Children[i];
            var start = resource.Start;
            var end = resource.End;
            var range = new MonacoRange(start.Line, start.Column, end.Line, end.Column);

            // Add all code lenses for this resource
            codeLenses.Add(CreateEditResourceCodeLens(localizer, range, i + indexOffset));
            codeLenses.Add(CreateValidateUnitCodeLens(localizer, range, i + indexOffset));
        }

        return codeLenses;
    }

    /// <summary>
    /// Create a code lens for validating a unit.
    /// </summary>
    /// <param name="localizer">The localizer.</param>
    /// <param name="range">The range of the code lens.</param>
    /// <param name="index">The index of the resource.</param>
    /// <returns>The created code lens.</returns>
    private static MonacoCodeLens CreateValidateUnitCodeLens(IStringLocalizer localizer, MonacoRange range, int index)
    {
        return new(range)
        {
            Command = new(localizer["CodeLens_ValidateText"])
            {
                Tooltip = localizer["CodeLens_ValidateTextTooltip"],
                Arguments = [new() { Id = ValidateUnitCommandId, Value = index }],
            },
        };
    }

    /// <summary>
    /// Create a code lens for editing a resource.
    /// </summary>
    /// <param name="localizer">The localizer.</param>
    /// <param name="range">The range of the code lens.</param>
    /// <param name="index">The index of the resource.</param>
    /// <returns>The created code lens.</returns>
    private static MonacoCodeLens CreateEditResourceCodeLens(IStringLocalizer localizer, MonacoRange range, int index)
    {
        return new(range)
        {
            Command = new(localizer["CodeLens_EditText"])
            {
                Tooltip = localizer["CodeLens_EditTextTooltip"],
                Arguments = [new() { Id = EditResourceCommandId, Value = index }],
            },
        };
    }
}

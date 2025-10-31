// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using WinGetStudio.Models;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

namespace WinGetStudio.ViewModels.Controls;

public sealed partial class ResourceSuggestionViewModel : ObservableObject
{
    private readonly ResourceSuggestion? _resourceSuggestion;

    /// <summary>
    /// Gets or sets the display name associated with the object.
    /// </summary>
    [ObservableProperty]
    public partial string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the text used for search operations.
    /// </summary>
    [ObservableProperty]
    public partial string? SearchText { get; set; }

    /// <summary>
    /// Gets or sets the collection of tags associated with the current entity.
    /// </summary>
    public List<string> Tags { get; set; }

    /// <summary>
    /// Gets the version of the resource suggestion.
    /// </summary>
    public string Version => _resourceSuggestion?.Version ?? string.Empty;

    /// <summary>
    /// Gets a value indicating whether the current instance represents a valid
    /// result, or is a user input placeholder.
    /// </summary>
    public bool IsResult => _resourceSuggestion != null;

    /// <summary>
    /// Gets the DSC module associated with the resource suggestion, if available.
    /// </summary>
    public DSCModule? Module => _resourceSuggestion?.Module;

    /// <summary>
    /// Gets the suggested DSC resource, if available.
    /// </summary>
    public DSCResource? Resource => _resourceSuggestion?.Resource;

    public ResourceSuggestionViewModel(ResourceSuggestion? resourceSuggestion = null)
    {
        _resourceSuggestion = resourceSuggestion;
        DisplayName = ResolveDisplayName(resourceSuggestion);
        Tags = GenerateTags(resourceSuggestion);
    }

    /// <summary>
    /// Generates tags based on the properties of the resource suggestion.
    /// </summary>
    /// <param name="resourceSuggestion">The resource suggestion.</param>
    /// <returns>A list of tags.</returns>
    private static List<string> GenerateTags(ResourceSuggestion? resourceSuggestion)
    {
        List<string> results = [];
        if (resourceSuggestion == null)
        {
            return results;
        }

        // Add the source as a tag
        if (!string.IsNullOrWhiteSpace(resourceSuggestion.Source))
        {
            results.Add(resourceSuggestion.Source);
        }

        // Add the DSC version as a tag
        if (resourceSuggestion.DSCVersion != DSCVersion.Unknown)
        {
            results.Add(resourceSuggestion.DSCVersion.ToString());
        }

        return results;
    }

    /// <summary>
    /// Resolves the display name for the resource suggestion.
    /// </summary>
    /// <param name="resourceSuggestion">The resource suggestion.</param>
    /// <returns>The resolved display name.</returns>
    private static string ResolveDisplayName(ResourceSuggestion? resourceSuggestion)
    {
        if (resourceSuggestion == null)
        {
            return string.Empty;
        }

        if (resourceSuggestion.IsModuleVirtual)
        {
            return $"{resourceSuggestion.ResourceName}";
        }

        return $"{resourceSuggestion.ModuleId}/{resourceSuggestion.ResourceName}";
    }
}

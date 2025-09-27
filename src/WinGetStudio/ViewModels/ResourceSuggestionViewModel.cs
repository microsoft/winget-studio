// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using WinGetStudio.Models;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

namespace WinGetStudio.ViewModels;

public sealed partial class ResourceSuggestionViewModel : ObservableObject
{
    private readonly ResourceSuggestion? _resourceSuggestion;

    [ObservableProperty]
    public partial string DisplayName { get; set; }

    [ObservableProperty]
    public partial string? SearchText { get; set; }

    public List<string> Tags { get; set; }

    public string Version => _resourceSuggestion?.Version ?? string.Empty;

    public bool IsResult => _resourceSuggestion != null;

    public ResourceSuggestionViewModel(ResourceSuggestion? resourceSuggestion = null)
    {
        _resourceSuggestion = resourceSuggestion;
        DisplayName = ResolveDisplayName(resourceSuggestion);
        Tags = GenerateTags(resourceSuggestion);
    }

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

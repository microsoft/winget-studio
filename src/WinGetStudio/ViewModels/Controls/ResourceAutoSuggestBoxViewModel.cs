// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using NuGet.Packaging;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Models;
using WinGetStudio.Models.Operations;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;
using WinGetStudio.Services.Operations.Extensions;

namespace WinGetStudio.ViewModels.Controls;

public sealed partial class ResourceAutoSuggestBoxViewModel : ObservableRecipient
{
    private readonly ResourceSuggestionViewModel _noResultsSuggestion;
    private readonly ConcurrentDictionary<string, ResourceSuggestionViewModel> _allSuggestions;
    private readonly IAppOperationHub _operationHub;
    private readonly IStringLocalizer<ResourceAutoSuggestBoxViewModel> _localizer;
    private readonly ILogger<ResourceAutoSuggestBoxViewModel> _logger;
    private readonly IDSCExplorer _explorer;

    private bool _isSearchTextSubmitted;

    [ObservableProperty]
    public partial string? SearchResourceText { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ReloadCommand))]
    public partial bool CanReload { get; set; }

    public ObservableCollection<ResourceSuggestionViewModel> SelectedSuggestions { get; }

    public ResourceAutoSuggestBoxViewModel(
        IAppOperationHub operationHub,
        IStringLocalizer<ResourceAutoSuggestBoxViewModel> localizer,
        IDSCExplorer explorer,
        ILogger<ResourceAutoSuggestBoxViewModel> logger)
    {
        _operationHub = operationHub;
        _localizer = localizer;
        _logger = logger;
        _explorer = explorer;
        _noResultsSuggestion = new();
        _allSuggestions = [];
        SelectedSuggestions = [_noResultsSuggestion];
    }

    [RelayCommand]
    private async Task OnLoadedAsync()
    {
        await LoadSuggestionsAsync();
    }

    [RelayCommand(CanExecute = nameof(CanReload))]
    private async Task OnReloadAsync()
    {
        await LoadSuggestionsAsync();
    }

    [RelayCommand]
    private async Task OnQuerySubmittedAsync(AutoSuggestBoxQuerySubmittedEventArgs arg)
    {
        if (arg.ChosenSuggestion is ResourceSuggestionViewModel suggestion
            && suggestion != _noResultsSuggestion
            && SearchResourceText != suggestion.DisplayName)
        {
            _isSearchTextSubmitted = true;
            SearchResourceText = suggestion.DisplayName;
        }

        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task OnSearchTextChangedAsync()
    {
        // Do not update suggestions if the search text was just submitted
        if (_isSearchTextSubmitted)
        {
            SelectedSuggestions.Clear();
            _isSearchTextSubmitted = false;
            return;
        }

        // Ensure the first suggestion is always the "no results" suggestion
        _noResultsSuggestion.DisplayName = SearchResourceText ?? string.Empty;
        _noResultsSuggestion.SearchText = SearchResourceText ?? string.Empty;
        if (SelectedSuggestions.Count == 0)
        {
            SelectedSuggestions.Add(_noResultsSuggestion);
        }

        // Remove all elements in the selected suggestions except the first one
        for (var i = SelectedSuggestions.Count - 1; i >= 1; i--)
        {
            SelectedSuggestions.RemoveAt(i);
        }

        // If the search text is empty or there are no suggestions, then the no
        // results suggestion is the only one to show
        if (string.IsNullOrWhiteSpace(SearchResourceText) || _allSuggestions.IsEmpty)
        {
            return;
        }

        // Find suggestions that match the search text
        var suggestionsResult = await Task.Run(() => _allSuggestions.Values
            .Where(s => s.DisplayName.Contains(SearchResourceText, StringComparison.OrdinalIgnoreCase))
            .Take(10)
            .ToList()
            .Select(s =>
            {
                s.SearchText = SearchResourceText;
                return s;
            }));

        // Add the matching suggestions to the selected suggestions
        SelectedSuggestions.AddRange(suggestionsResult);
    }

    /// <summary>
    /// Loads DSC resource suggestions asynchronously.
    /// </summary>
    private async Task LoadSuggestionsAsync()
    {
        CanReload = false;
        _allSuggestions.Clear();
        await _operationHub.RunWithProgressAsync(
            props => props with { Message = _localizer["LoadingDSCResources_LoadingMessage"] },
            async (context, factory) =>
            {
                await foreach (var catalog in _explorer.GetModuleCatalogsAsync())
                {
                    foreach (var module in catalog.Modules.Values)
                    {
                        foreach (var resource in module.Resources.Values)
                        {
                            var suggestion = new ResourceSuggestion(module, resource);
                            var viewModel = new ResourceSuggestionViewModel(suggestion);
                            _allSuggestions.TryAdd(viewModel.DisplayName.ToLowerInvariant(), viewModel);
                        }
                    }
                }

                context.Complete(props => props with { Message = _localizer["LoadingDSCResources_CompletedMessage"] });
            },
            canCancel: false);
        CanReload = true;
    }

    /// <summary>
    /// Handles the exploration of a selected DSC resource asynchronously.
    /// </summary>
    /// <returns>The selected DSC resource, or null if not found.</returns>
    public async Task<OperationResult<DSCResource>> OnExploreAsync()
    {
        return await _operationHub.RunWithProgressAsync<DSCResource>(
            props => props with { Message = _localizer["ExploreDSCResource_LoadingMessage"] },
            async (ctx, factory) =>
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(SearchResourceText))
                    {
                        if (_allSuggestions.TryGetValue(SearchResourceText.ToLowerInvariant(), out var selectedSuggestion)
                            && selectedSuggestion.Module != null
                            && selectedSuggestion.Resource != null)
                        {
                            // If the module is not enriched, enrich it with resource details
                            if (!selectedSuggestion.Module.IsEnriched)
                            {
                                await _explorer.EnrichModuleWithResourceDetailsAsync(selectedSuggestion.Module);
                            }

                            // If the module is still not enriched, show a warning and return null
                            if (!selectedSuggestion.Module.IsEnriched)
                            {
                                ctx.Warn(props => props with { Message = _localizer["ExploreDSCResource_NotFoundMessage"] });
                                return new() { Result = null };
                            }

                            ctx.Complete(props => props with { Message = _localizer["ExploreDSCResource_CompletedMessage"] });
                            return new() { Result = selectedSuggestion.Resource };
                        }
                        else
                        {
                            ctx.Warn(props => props with { Message = _localizer["ExploreDSCResource_NotFoundMessage"] });
                        }
                    }

                    return new() { Result = null };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while exploring the DSC resource.");
                    ctx.Fail(props => props with { Message = _localizer["ExploreDSCResource_ErrorMessage"] });
                    return new() { Error = null };
                }
            });
    }
}

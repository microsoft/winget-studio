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
using WinGetStudio.Models;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;
using WingetStudio.Services.VisualFeedback.Contracts;
using WingetStudio.Services.VisualFeedback.Models;

namespace WinGetStudio.ViewModels;

public delegate ValidationViewModel ValidationViewModelFactory();

public partial class ValidationViewModel : ObservableRecipient
{
    private readonly IDSC _dsc;
    private readonly IDSCExplorer _dscExplorer;
    private readonly IUIFeedbackService _ui;
    private readonly IStringLocalizer<ValidationViewModel> _localizer;
    private readonly ILogger<ValidationViewModel> _logger;
    private readonly ResourceSuggestionViewModel _noResultsSuggestion;
    private readonly ConcurrentDictionary<string, ResourceSuggestionViewModel> _allSuggestions = [];

    private bool _isSearchTextSubmitted;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ReloadCommand))]
    public partial bool CanReload { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GetCommand))]
    [NotifyCanExecuteChangedFor(nameof(SetCommand))]
    [NotifyCanExecuteChangedFor(nameof(TestCommand))]
    public partial bool CanExecuteDSCOperation { get; set; } = true;

    [ObservableProperty]
    public partial string? SearchResourceText { get; set; }

    [ObservableProperty]
    public partial string OutputText { get; set; } = string.Empty;

    public ObservableCollection<ResourceSuggestionViewModel> SelectedSuggestions { get; }

    public ValidationViewModel(
        IDSC dsc,
        IDSCExplorer dscExplorer,
        IUIFeedbackService ui,
        IStringLocalizer<ValidationViewModel> localizer,
        ILogger<ValidationViewModel> logger)
    {
        _dsc = dsc;
        _dscExplorer = dscExplorer;
        _ui = ui;
        _localizer = localizer;
        _logger = logger;
        _noResultsSuggestion = new();
        SelectedSuggestions = [_noResultsSuggestion];
    }

    /// <summary>
    /// Retrieves the current configuration unit from the system asynchronously.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecuteDSCOperation))]
    private async Task OnGetAsync()
    {
        await RunDscOperationAsync(async () =>
        {
            await Task.CompletedTask;
            return null;
        });
    }

    /// <summary>
    /// Sets the current machine state to the specified configuration unit asynchronously.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecuteDSCOperation))]
    private async Task OnSetAsync()
    {
        await RunDscOperationAsync(async () =>
        {
            await Task.CompletedTask;
            return null;
        });
    }

    /// <summary>
    /// Tests whether the current machine state matches the specified configuration unit asynchronously.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecuteDSCOperation))]
    private async Task OnTestAsync()
    {
        await RunDscOperationAsync(async () =>
        {
            await Task.CompletedTask;
            return null;
        });
    }

    /// <summary>
    /// Runs a DSC operation while managing UI feedback.
    /// </summary>
    /// <param name="action">The DSC operation to execute.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task RunDscOperationAsync(Func<Task<IDSCUnitResultInformation?>> action)
    {
        try
        {
            CanExecuteDSCOperation = false;
            _ui.ShowTaskProgress();
            var result = await action();
            if (result != null && !result.IsOk)
            {
                var title = $"0x{result.ResultCode.HResult:X}";
                List<string> messageList = [result.Description, result.Details];
                var message = string.Join(Environment.NewLine, messageList.Where(s => !string.IsNullOrEmpty(s)));
                _ui.ShowTimedNotification(title, message, NotificationMessageSeverity.Error);
            }
        }
        catch (Exception e)
        {
            _ui.ShowTimedNotification(e.Message, NotificationMessageSeverity.Error);
            _logger.LogError(e, "An error occurred while executing a DSC operation.");
        }
        finally
        {
            _ui.HideTaskProgress();
            CanExecuteDSCOperation = true;
        }
    }

    [RelayCommand]
    private async Task OnLoadedAsync()
    {
        await LoadSuggestionsAsync();
    }

    /// <summary>
    /// Handles the exploration of a selected DSC resource asynchronously.
    /// </summary>
    /// <returns>The selected DSC resource, or null if not found.</returns>
    public async Task<DSCResource?> OnExploreAsync()
    {
        try
        {
            _ui.ShowTaskProgress();
            if (!string.IsNullOrWhiteSpace(SearchResourceText))
            {
                if (_allSuggestions.TryGetValue(SearchResourceText.ToLowerInvariant(), out var selectedSuggestion)
                    && selectedSuggestion.Module != null
                    && selectedSuggestion.Resource != null)
                {
                    // If the module is not enriched, enrich it with resource details
                    if (!selectedSuggestion.Module.IsEnriched)
                    {
                        await _dscExplorer.EnrichModuleWithResourceDetailsAsync(selectedSuggestion.Module);
                    }

                    // If the module is still not enriched, show a warning and return null
                    if (!selectedSuggestion.Module.IsEnriched)
                    {
                        _ui.ShowTimedNotification(_localizer["ResourceInfoNotFoundMessage"], NotificationMessageSeverity.Warning);
                        return null;
                    }

                    return selectedSuggestion.Resource;
                }
                else
                {
                    _ui.ShowTimedNotification(_localizer["ResourceInfoNotFoundMessage"], NotificationMessageSeverity.Warning);
                }
            }

            return null;
        }
        finally
        {
            _ui.HideTaskProgress();
        }
    }

    [RelayCommand(CanExecute = nameof(CanReload))]
    private async Task OnReloadAsync()
    {
        await LoadSuggestionsAsync();
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

    /// <summary>
    /// Loads DSC resource suggestions asynchronously.
    /// </summary>
    private async Task LoadSuggestionsAsync()
    {
        CanReload = false;
        _allSuggestions.Clear();
        _ui.ShowTaskProgress();
        _ui.ShowTimedNotification(_localizer["LoadingDSCResourcesMessage"], NotificationMessageSeverity.Informational);

        await foreach (var catalog in _dscExplorer.GetModuleCatalogsAsync())
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

        _ui.ShowTimedNotification(_localizer["CompletedLoadingDSCResourcesMessage"], NotificationMessageSeverity.Informational);
        _ui.HideTaskProgress();
        CanReload = true;
    }
}

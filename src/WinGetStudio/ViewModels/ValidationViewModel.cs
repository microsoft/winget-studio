// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Localization;
using Microsoft.UI.Xaml.Controls;
using NuGet.Packaging;
using Windows.Foundation.Collections;
using WinGetStudio.Contracts.ViewModels;
using WinGetStudio.Models;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;
using WingetStudio.Services.VisualFeedback.Contracts;
using WingetStudio.Services.VisualFeedback.Models;

namespace WinGetStudio.ViewModels;

public delegate ValidationViewModel ValidationViewModelFactory();

public partial class ValidationViewModel : ObservableRecipient, INavigationAware
{
    private readonly IDSC _dsc;
    private readonly IDSCExplorer _dscExplorer;
    private readonly IUIFeedbackService _ui;
    private readonly IStringLocalizer<ValidationViewModel> _localizer;
    private readonly ResourceSuggestionViewModel _noResultsSuggestion;
    private readonly List<ResourceSuggestionViewModel> _allSuggestions = [];

    private bool _isSearchTextSubmitted;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ReloadCommand))]
    public partial bool AreSuggestionsLoaded { get; set; }

    [ObservableProperty]
    public partial string? SearchResourceText { get; set; }

    [ObservableProperty]
    public partial string RawData { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool ActionsEnabled { get; set; } = true;

    public ObservableCollection<ResourceSuggestionViewModel> SelectedSuggestions { get; }

    public bool IsPropertiesEmpty => Properties.Count == 0;

    public ObservableCollection<ConfigurationProperty> Properties { get; } = new();

    public ValidationViewModel(
        IDSC dsc,
        IDSCExplorer dscExplorer,
        IUIFeedbackService ui,
        IStringLocalizer<ValidationViewModel> localizer)
    {
        _dsc = dsc;
        _dscExplorer = dscExplorer;
        _ui = ui;
        _localizer = localizer;
        _noResultsSuggestion = new();
        SelectedSuggestions = [_noResultsSuggestion];

        Properties.CollectionChanged += (_, __) =>
        {
            OnPropertyChanged(nameof(IsPropertiesEmpty));
        };
    }

    public void OnNavigatedTo(object parameter)
    {
        if (parameter is IDSCUnit u)
        {
            var moduleName = u.ModuleName;
            var type = u.Type;
            SearchResourceText = moduleName == string.Empty ? type : $"{moduleName}/{type}";

            foreach (var kvp in u.Settings)
            {
                if (kvp.Value is string s)
                {
                    Properties.Add(new(kvp.Key, new StringValue(s)));
                }
                else if (kvp.Value is bool b)
                {
                    Properties.Add(new(kvp.Key, new BooleanValue(b)));
                }
                else if (kvp.Value is double d)
                {
                    Properties.Add(new(kvp.Key, new NumberValue(d)));
                }
                else if (kvp.Value is ValueSet v)
                {
                    ObservableCollection<ConfigurationProperty> nestedProperties = new();
                    ConvertYamlToUIHelper(nestedProperties, v);
                    Properties.Add(new(kvp.Key, new ObjectValue(nestedProperties)));
                }
            }
        }
    }

    public void OnNavigatedFrom()
    {
        // No-op
    }

    /// <summary>
    /// Creates a new <see cref="ConfigurationUnitModel"/> instance based on the current module name and properties.
    /// </summary>
    /// <returns>Newly created <see cref="ConfigurationUnitModel"/> instance.</returns>
    private ConfigurationUnitModel CreateConfigurationUnitModel()
    {
        ConfigurationUnitModel unit = new();
        unit.Type = SearchResourceText;
        ConfigurationPropertiesToValueSet(unit.Settings, Properties);
        return unit;
    }

    private void ConfigurationPropertiesToValueSet(ValueSet settings, ObservableCollection<ConfigurationProperty> properties)
    {
        foreach (var property in properties)
        {
            if (property.Value.Value is string s)
            {
                settings.Add(new(property.Name, s));
            }
            else if (property.Value.Value is bool b)
            {
                settings.Add(new(property.Name, b));
            }
            else if (property.Value.Value is double d)
            {
                settings.Add(new(property.Name, d));
            }
            else if (property.Value.Value is ObservableCollection<ConfigurationProperty> nestedProperties)
            {
                ValueSet nestedSettings = new();
                ConfigurationPropertiesToValueSet(nestedSettings, nestedProperties);
                settings.Add(new(property.Name, nestedSettings));
            }
        }
    }

    /// <summary>
    /// Converts YAML data into a user interface representation asynchronously.
    /// </summary>
    /// <remarks>This method processes the raw YAML data and updates the UI-related properties accordingly.
    /// It clears the existing properties and populates them based on the parsed YAML configuration.</remarks>
    [RelayCommand]
    private void OnConvertYamlToUI()
    {
        var unit = CreateConfigurationUnitModel();
        if (unit.TryLoad(RawData))
        {
            SearchResourceText = unit.Type;
            Properties.Clear();
            ConvertYamlToUIHelper(Properties, unit.Settings);
        }
        else
        {
            // TODO implement error handling
        }
    }

    /// <summary>
    /// Helper function to convert a <see cref="ValueSet"/> into an observable collection of configuration properties.
    /// </summary>
    /// <remarks>This method recursively processes nested <see cref="ValueSet"/> instances, converting them
    /// into collections of configuration properties. Each key-value pair in the <paramref name="settings"/> is mapped
    /// to a corresponding <see cref="ConfigurationProperty"/> based on the type of the value.</remarks>
    /// <param name="properties">The collection to populate with configuration properties derived from the <paramref name="settings"/>.</param>
    /// <param name="settings">A <see cref="ValueSet"/> containing key-value pairs to be converted into configuration properties. The values
    /// can be of type <see langword="string"/>, <see langword="bool"/>, <see langword="double"/>, or nested <see
    /// cref="ValueSet"/>.</param>
    private void ConvertYamlToUIHelper(ObservableCollection<ConfigurationProperty> properties, ValueSet settings)
    {
        foreach (var kvp in settings)
        {
            if (kvp.Value is string s)
            {
                properties.Add(new(kvp.Key, new StringValue(s)));
            }
            else if (kvp.Value is bool b)
            {
                properties.Add(new(kvp.Key, new BooleanValue(b)));
            }
            else if (kvp.Value is double d)
            {
                properties.Add(new(kvp.Key, new NumberValue(d)));
            }
            else if (kvp.Value is ValueSet v)
            {
                ObservableCollection<ConfigurationProperty> nestedProperties = new();
                ConvertYamlToUIHelper(nestedProperties, v);
                properties.Add(new(kvp.Key, new ObjectValue(nestedProperties)));
            }
        }
    }

    /// <summary>
    /// Retrieves the current configuration unit from the system asynchronously.
    /// </summary>
    [RelayCommand]
    private async Task OnGetAsync()
    {
        await RunDscOperationAsync(async () =>
        {
            ActionsEnabled = false;
            var unit = CreateConfigurationUnitModel();
            await _dsc.DscGet(unit);
            RawData = unit.ToYaml();
            ActionsEnabled = true;
        });
    }

    /// <summary>
    /// Sets the current machine state to the specified configuration unit asynchronously.
    /// </summary>
    [RelayCommand]
    private async Task OnSetAsync()
    {
        await RunDscOperationAsync(async () =>
        {
            ActionsEnabled = false;
            var unit = CreateConfigurationUnitModel();
            await _dsc.DscSet(unit);
            ActionsEnabled = true;
        });
    }

    /// <summary>
    /// Tests whether the current machine state matches the specified configuration unit asynchronously.
    /// </summary>
    [RelayCommand]
    private async Task OnTestAsync()
    {
        await RunDscOperationAsync(async () =>
        {
            var unit = CreateConfigurationUnitModel();
            await _dsc.DscTest(unit);
            if (unit.TestResult)
            {
                var message = _localizer["Notification_MachineInDesiredState"];
                _ui.ShowTimedNotification(message, NotificationMessageSeverity.Success);
            }
            else
            {
                var message = _localizer["Notification_MachineNotInDesiredState"];
                _ui.ShowTimedNotification(message, NotificationMessageSeverity.Error);
            }
        });
    }

    /// <summary>
    /// Runs a DSC operation while managing UI feedback.
    /// </summary>
    /// <param name="action">The DSC operation to execute.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task RunDscOperationAsync(Func<Task> action)
    {
        ActionsEnabled = false;
        _ui.ShowTaskProgress();
        await action();
        _ui.HideTaskProgress();
        ActionsEnabled = true;
    }

    [RelayCommand]
    private async Task OnLoadedAsync()
    {
        await LoadSuggestionsAsync();
    }

    [RelayCommand]
    private async Task OnExploreAsync()
    {
        _ui.ShowTaskProgress();

        if (!string.IsNullOrWhiteSpace(SearchResourceText))
        {
            await Task.CompletedTask;
        }

        _ui.HideTaskProgress();
    }

    [RelayCommand(CanExecute = nameof(AreSuggestionsLoaded))]
    private async Task OnReloadAsync()
    {
        await LoadSuggestionsAsync();
    }

    [RelayCommand]
    private async Task OnSearchTextChangedAsync()
    {
        if (_isSearchTextSubmitted)
        {
            SelectedSuggestions.Clear();
            _isSearchTextSubmitted = false;
            return;
        }

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

        if (string.IsNullOrWhiteSpace(SearchResourceText) || _allSuggestions.Count == 0)
        {
            return;
        }

        var suggestionsResult = await Task.Run(() => _allSuggestions
            .Where(s => s.DisplayName.Contains(SearchResourceText, StringComparison.OrdinalIgnoreCase))
            .Take(10)
            .ToList()
            .Select(s =>
            {
                s.SearchText = SearchResourceText;
                return s;
            }));

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

    private async Task LoadSuggestionsAsync()
    {
        AreSuggestionsLoaded = false;
        _allSuggestions.Clear();
        _ui.ShowTaskProgress();
        _ui.ShowTimedNotification("Loading DSC resources ...", NotificationMessageSeverity.Informational);

        var catalogs = await _dscExplorer.GetModuleCatalogsAsync();
        foreach (var catalog in catalogs)
        {
            foreach (var module in catalog.Modules.Values)
            {
                foreach (var resource in module.Resources.Values)
                {
                    var suggestion = new ResourceSuggestion(module, resource);
                    var viewModel = new ResourceSuggestionViewModel(suggestion);
                    _allSuggestions.Add(viewModel);
                }
            }
        }

        _ui.ShowTimedNotification("Completed loading DSC resources.", NotificationMessageSeverity.Informational);
        _ui.HideTaskProgress();
        AreSuggestionsLoaded = true;
    }
}

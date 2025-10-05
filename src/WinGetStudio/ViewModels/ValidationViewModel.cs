// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using NuGet.Packaging;
using Windows.Foundation.Collections;
using WinGetStudio.Contracts.ViewModels;
using WinGetStudio.Models;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;
using WinGetStudio.Services.DesiredStateConfiguration.Models;
using WingetStudio.Services.VisualFeedback.Contracts;
using WingetStudio.Services.VisualFeedback.Models;
using YamlDotNet.Serialization;

namespace WinGetStudio.ViewModels;

public delegate ValidationViewModel ValidationViewModelFactory();

public partial class ValidationViewModel : ObservableRecipient, INavigationAware
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

    private readonly IDSC _dsc;
    private readonly IDSCExplorer _dscExplorer;
    private readonly IUIFeedbackService _ui;
    private readonly IStringLocalizer<ValidationViewModel> _localizer;
    private readonly ResourceSuggestionViewModel _noResultsSuggestion;
    private readonly ConcurrentDictionary<string, ResourceSuggestionViewModel> _allSuggestions = [];
    private readonly ILogger<ValidationViewModel> _logger;

    private bool _isSearchTextSubmitted;
    private ConfigurationUnitModel? _currentUnit;

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
    public partial string RawData { get; set; } = string.Empty;

    [ObservableProperty]
    public partial ResultLanguageMode SelectedResultLanguageMode { get; set; }

    [ObservableProperty]
    public partial ResultSchemaVersion SelectedResultSchemaVersion { get; set; }

    public ObservableCollection<ResourceSuggestionViewModel> SelectedSuggestions { get; }

    public ObservableCollection<ConfigurationProperty> Properties { get; } = new();

    public List<ResultLanguageMode> ResultLanguageModes { get; } = [..Enum.GetValues<ResultLanguageMode>()];

    public List<ResultSchemaVersion> ResultSchemaVersions { get; } = [..Enum.GetValues<ResultSchemaVersion>()];

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
        foreach (var property in Properties)
        {
            var keyValue = property.ToKeyValuePair();
            unit.Settings.TryAdd(keyValue.Key, keyValue.Value);
        }

        return unit;
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
    [RelayCommand(CanExecute = nameof(CanExecuteDSCOperation))]
    private async Task OnGetAsync()
    {
        await RunDscOperationAsync(async () =>
        {
            _currentUnit = CreateConfigurationUnitModel();
            var result = await _dsc.GetUnitAsync(_currentUnit);
            await OnResultModeChangedAsync();
            return result.ResultInformation;
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
            var unit = CreateConfigurationUnitModel();
            var result = await _dsc.SetUnitAsync(unit);
            return result.ResultInformation;
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
            var unit = CreateConfigurationUnitModel();
            var result = await _dsc.TestUnitAsync(unit);
            if (result.ResultInformation == null || result.ResultInformation.IsOk)
            {
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
            }

            return result.ResultInformation;
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
            var message = $"Error occurred: {e.Message}";
            _ui.ShowTimedNotification(message, NotificationMessageSeverity.Error);
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

    [RelayCommand]
    private async Task OnResultModeChangedAsync()
    {
        if (_currentUnit != null)
        {
            if (SelectedResultSchemaVersion == ResultSchemaVersion.V0_1)
            {
                var v0_1 = _currentUnit.ToWinGetConfigurationV0_1();
                RawData = SelectedResultLanguageMode == ResultLanguageMode.JSON ? ToJson(v0_1) : ToYaml(v0_1);
            }
            else if (SelectedResultSchemaVersion == ResultSchemaVersion.V0_2)
            {
                var v0_2 = _currentUnit.ToWinGetConfigurationV0_2();
                RawData = SelectedResultLanguageMode == ResultLanguageMode.JSON ? ToJson(v0_2) : ToYaml(v0_2);
            }
            else if (SelectedResultSchemaVersion == ResultSchemaVersion.V0_3)
            {
                var v0_3 = _currentUnit.ToWinGetConfigurationV0_3();
                RawData = SelectedResultLanguageMode == ResultLanguageMode.JSON ? ToJson(v0_3) : ToYaml(v0_3);
            }
        }

        await Task.CompletedTask;
    }

    private string ToJson<T>(T obj)
        where T : class
    {
        return JsonSerializer.Serialize(obj, _jsonOptions);
    }

    public string ToYaml<T>(T obj)
        where T : class
    {
        // 1. Parse to JSON
        var json = ToJson<T>(obj);
        using var reader = new StringReader(json);
        var deserializer = new DeserializerBuilder().WithAttemptingUnquotedStringTypeDeserialization().Build();
        var yamlObj = deserializer.Deserialize(reader);

        // 2. Serialize to YAML
        var serializer = new SerializerBuilder().WithQuotingNecessaryStrings().Build();
        return serializer.Serialize(yamlObj);
    }
}

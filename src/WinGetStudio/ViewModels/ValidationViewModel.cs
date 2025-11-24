// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Management.Configuration;
using WinGetStudio.Contracts.ViewModels;
using WinGetStudio.Models;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Exceptions;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Extensions;
using WinGetStudio.Services.DesiredStateConfiguration.Models;
using WingetStudio.Services.VisualFeedback.Contracts;
using WingetStudio.Services.VisualFeedback.Models;

namespace WinGetStudio.ViewModels;

public partial class ValidationViewModel : ObservableRecipient, INavigationAware
{
    private readonly IDSC _dsc;
    private readonly IUIFeedbackService _ui;
    private readonly IStringLocalizer<ValidationViewModel> _localizer;
    private readonly ILogger<ValidationViewModel> _logger;
    private readonly UnitViewModelFactory _unitFactory;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GetCommand))]
    [NotifyCanExecuteChangedFor(nameof(SetCommand))]
    [NotifyCanExecuteChangedFor(nameof(TestCommand))]
    public partial bool CanExecuteDSCOperation { get; set; } = true;

    [ObservableProperty]
    public partial string? SearchResourceText { get; set; }

    [ObservableProperty]
    public partial string? OutputText { get; set; }

    [ObservableProperty]
    public partial string? SettingsText { get; set; }

    public ValidationViewModel(
        IDSC dsc,
        IUIFeedbackService ui,
        IStringLocalizer<ValidationViewModel> localizer,
        ILogger<ValidationViewModel> logger,
        UnitViewModelFactory unitFactory)
    {
        _dsc = dsc;
        _ui = ui;
        _localizer = localizer;
        _logger = logger;
        _unitFactory = unitFactory;
    }

    public void OnNavigatedTo(object parameter)
    {
        if (parameter is ValidateUnitNavigationContext context)
        {
            SearchResourceText = context.UnitToValidate.Title;
            SettingsText = context.UnitToValidate.SettingsText;
        }
    }

    public void OnNavigatedFrom()
    {
        // No-op
    }

    /// <summary>
    /// Retrieves the current configuration unit from the system asynchronously.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecuteDSCOperation))]
    private async Task OnGetAsync()
    {
        await RunDscOperationAsync(async (dscUnit) =>
        {
            var result = await _dsc.GetUnitAsync(dscUnit);
            if (result.ResultInformation?.IsOk ?? true)
            {
                OutputText = result.Settings.ToYaml();
            }

            return result.ResultInformation;
        });
    }

    /// <summary>
    /// Sets the current machine state to the specified configuration unit asynchronously.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecuteDSCOperation))]
    private async Task OnSetAsync()
    {
        await RunDscOperationAsync(async (dscUnit) =>
        {
            var inputSettings = dscUnit.Settings.DeepCopy();
            var setResult = await _dsc.SetUnitAsync(dscUnit);
            var afterResult = await _dsc.GetUnitAsync(dscUnit);

            // Merge input with actual state for a complete "before" view
            // Note: This works well for simple properties but has limitations with deeply nested objects
            var beforeSettings = MergeSettings(inputSettings, afterResult.Settings);
            var beforeState = beforeSettings.ToYaml().TrimEnd();
            var afterState = afterResult.Settings.ToYaml().TrimEnd();

            var action = setResult.PreviouslyInDesiredState ? "None" : "Partial";

            var changedProperties = GetChangedProperties(beforeSettings, afterResult.Settings);

            // Build similar output as DSC
            var changedPropsStr = changedProperties.Count == 0
                ? "[]"
                : Environment.NewLine + string.Join(Environment.NewLine, changedProperties.Select(p => $"- {p}"));

            var output = $"beforeState:{IndentYaml(beforeState, 2)}{Environment.NewLine}afterState:{IndentYaml(afterState, 2)}{Environment.NewLine}action: {action}{Environment.NewLine}changedProperties: {changedPropsStr}";

            if (setResult.ResultInformation?.IsOk ?? true)
            {
                OutputText = output;
            }

            return setResult.ResultInformation;
        });
    }

    /// <summary>
    /// Merges input settings with actual state to create a complete "before" view.
    /// This overlays input values on top of the actual state.
    /// Note: Deep merging of nested DSCPropertySet objects is complex and may not always
    /// produce accurate results when the input contains partial nested objects.
    /// This works best for resources with simple, flat property structures.
    /// </summary>
    private DSCPropertySet MergeSettings(DSCPropertySet input, DSCPropertySet actual)
    {
        var merged = new DSCPropertySet();

        // Copy all properties from actual state
        foreach (var kvp in actual)
        {
            merged[kvp.Key] = kvp.Value is DSCPropertySet nested ? nested.DeepCopy() : kvp.Value;
        }

        // Overlay with input values (this may overwrite entire nested objects)
        foreach (var kvp in input)
        {
            merged[kvp.Key] = kvp.Value is DSCPropertySet nested ? nested.DeepCopy() : kvp.Value;
        }

        return merged;
    }

    /// <summary>
    /// Indents YAML content by the specified number of spaces.
    /// </summary>
    private string IndentYaml(string yaml, int spaces)
    {
        var indent = new string(' ', spaces);
        var lines = yaml.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);
        var indented = string.Join(Environment.NewLine, lines.Select(line => string.IsNullOrWhiteSpace(line) ? line : indent + line));
        return Environment.NewLine + indented;
    }

    /// <summary>
    /// Compares two property sets and returns a list of changed property names.
    /// </summary>
    private List<string> GetChangedProperties(DSCPropertySet before, DSCPropertySet after)
    {
        var changedProperties = new List<string>();

        // Check for modified or added properties
        foreach (var kvp in after)
        {
            if (!before.TryGetValue(kvp.Key, out var beforeValue) || !AreValuesEqual(beforeValue, kvp.Value))
            {
                changedProperties.Add(kvp.Key);
            }
        }

        // Check for removed properties
        foreach (var kvp in before)
        {
            if (!after.ContainsKey(kvp.Key))
            {
                changedProperties.Add(kvp.Key);
            }
        }

        return changedProperties;
    }

    /// <summary>
    /// Compares two values for equality, handling nested objects.
    /// </summary>
    private bool AreValuesEqual(object value1, object value2)
    {
        if (value1 == null && value2 == null)
        {
            return true;
        }

        if (value1 == null || value2 == null)
        {
            return false;
        }

        if (value1 is DSCPropertySet dict1 && value2 is DSCPropertySet dict2)
        {
            if (dict1.Count != dict2.Count)
            {
                return false;
            }

            return dict1.All(kvp => dict2.TryGetValue(kvp.Key, out var v2) && AreValuesEqual(kvp.Value, v2));
        }

        // Handle collections (but not strings or dictionaries)
        if (value1 is not string && value2 is not string &&
            value1 is not DSCPropertySet && value2 is not DSCPropertySet)
        {
            try
            {
                if (value1 is System.Collections.IList list1 && value2 is System.Collections.IList list2)
                {
                    if (list1.Count != list2.Count)
                    {
                        return false;
                    }

                    for (int i = 0; i < list1.Count; i++)
                    {
                        if (!AreValuesEqual(list1[i]!, list2[i]!))
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }
            catch
            {
                // If enumeration fails, fall back to simple equality
            }
        }

        return value1.Equals(value2);
    }

    /// <summary>
    /// Tests whether the current machine state matches the specified configuration unit asynchronously.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecuteDSCOperation))]
    private async Task OnTestAsync()
    {
        await RunDscOperationAsync(async (dscUnit) =>
        {
            var result = await _dsc.TestUnitAsync(dscUnit);
            if (result.TestResult == ConfigurationTestResult.Positive)
            {
                _ui.ShowTimedNotification(_localizer["Notification_MachineInDesiredState"], NotificationMessageSeverity.Success);
            }
            else
            {
                _ui.ShowTimedNotification(_localizer["Notification_MachineNotInDesiredState"], NotificationMessageSeverity.Error);
            }

            return result.ResultInformation;
        });
    }

    /// <summary>
    /// Runs a DSC operation while managing UI feedback.
    /// </summary>
    /// <param name="action">The DSC operation to execute.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task RunDscOperationAsync(Func<IDSCUnit, Task<IDSCUnitResultInformation?>> action)
    {
        try
        {
            CanExecuteDSCOperation = false;
            _ui.ShowTaskProgress();
            var unit = await CreateUnitAsync();
            var result = await action(unit);
            if (result != null && !result.IsOk)
            {
                var title = $"0x{result.ResultCode.HResult:X}";
                List<string> messageList = [result.Description, result.Details];
                var message = string.Join(Environment.NewLine, messageList.Where(s => !string.IsNullOrEmpty(s)));
                _ui.ShowTimedNotification(title, message, NotificationMessageSeverity.Error);
            }
        }
        catch (OpenConfigurationSetException ex)
        {
            _logger.LogError(ex, "An error occurred while opening the DSC configuration set.");
            _ui.ShowTimedNotification(ex.GetErrorMessage(_localizer), NotificationMessageSeverity.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while executing a DSC operation.");
            _ui.ShowTimedNotification(ex.Message, NotificationMessageSeverity.Error);
        }
        finally
        {
            _ui.HideTaskProgress();
            CanExecuteDSCOperation = true;
        }
    }

    /// <summary>
    /// Creates a DSC unit from the current state.
    /// </summary>
    /// <returns>The created DSC unit.</returns>
    private async Task<IDSCUnit> CreateUnitAsync()
    {
        var unit = _unitFactory();
        unit.Title = SearchResourceText ?? string.Empty;
        unit.Settings = DSCPropertySet.FromYaml(SettingsText ?? string.Empty);
        var dscFile = DSCFile.CreateVirtual(unit.ToConfigurationV3().ToYaml());
        var dscSet = await _dsc.OpenConfigurationSetAsync(dscFile);
        return dscSet.Units[0];
    }
}

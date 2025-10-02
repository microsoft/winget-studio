// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Localization;
using Microsoft.UI.Xaml;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;
using WinGetStudio.Services.Settings;
using WinGetStudio.Services.Settings.Contracts;
using WinGetStudio.Services.Settings.Models;
using WingetStudio.Services.VisualFeedback.Contracts;
using WingetStudio.Services.VisualFeedback.Models;

namespace WinGetStudio.ViewModels;

public partial class SettingsViewModel : ObservableRecipient
{
    private readonly IAppInfoService _appInfoService;
    private readonly IUserSettings _userSettings;
    private readonly IAppSettingsService _appSettings;
    private readonly IUIDispatcher _dispatcher;
    private readonly IDSCExplorer _dscExplorer;
    private readonly IUIFeedbackService _ui;
    private readonly IStringLocalizer<SettingsViewModel> _localizer;

    [ObservableProperty]
    public partial ElementTheme ElementTheme { get; set; }

    [ObservableProperty]
    public partial string VersionDescription { get; set; }

    [ObservableProperty]
    public partial bool DisableTelemetry { get; set; }

    public SettingsViewModel(
        IAppInfoService appInfoService,
        IUserSettings userSettings,
        IAppSettingsService appSettings,
        IUIDispatcher dispatcher,
        IDSCExplorer dscExplorer,
        IUIFeedbackService ui,
        IStringLocalizer<SettingsViewModel> localizer)
    {
        _appInfoService = appInfoService;
        _userSettings = userSettings;
        _appSettings = appSettings;
        _dispatcher = dispatcher;
        _dscExplorer = dscExplorer;
        _ui = ui;
        _localizer = localizer;

        // Initialize settings
        VersionDescription = GetVersionDescription();
        RefreshSettings();
    }

    /// <summary>
    /// Gets the version description of the app.
    /// </summary>
    /// <returns>The version description.</returns>
    private string GetVersionDescription()
    {
        return $"{_appInfoService.GetAppNameLocalized()} - {_appInfoService.GetAppVersion()}";
    }

    [RelayCommand]
    private async Task SwitchThemeAsync(ElementTheme theme)
    {
        await _userSettings.SaveAsync(settings => settings.Theme = theme.ToString());
    }

    [RelayCommand]
    private async Task ToggleTelemetryAsync()
    {
        await _userSettings.SaveAsync(settings => settings.Telemetry.Disable = DisableTelemetry);
    }

    [RelayCommand]
    private async Task ClearModuleCatalogsCacheAsync()
    {
        _ui.ShowTaskProgress();
        await _dscExplorer.ClearCacheAsync();
        _ui.ShowTimedNotification(_localizer["CacheClearedMessage"], NotificationMessageSeverity.Success);
        _ui.HideTaskProgress();
    }

    [RelayCommand]
    private void OnLoaded()
    {
        _userSettings.SettingsChanged += OnSettingsChanged;
    }

    [RelayCommand]
    private void OnUnloaded()
    {
        _userSettings.SettingsChanged -= OnSettingsChanged;
    }

    /// <summary>
    /// Handles settings changes.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event args.</param>
    private async void OnSettingsChanged(object? sender, GeneralSettings e)
    {
        await _dispatcher.EnqueueAsync(RefreshSettings);
    }

    /// <summary>
    /// Refreshes the settings from the user settings.
    /// </summary>
    private void RefreshSettings()
    {
        ElementTheme = _appSettings.GetFeature<ThemeFeatureSettings>().Theme;
        DisableTelemetry = _appSettings.GetFeature<TelemetryFeatureSettings>().IsDisabled;
    }
}

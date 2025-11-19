// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Localization;
using Microsoft.UI.Xaml;
using Windows.System;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Models;
using WinGetStudio.Services;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;
using WinGetStudio.Services.Operations.Extensions;
using WinGetStudio.Services.Settings;
using WinGetStudio.Services.Settings.Contracts;
using WinGetStudio.Services.Settings.Models;

namespace WinGetStudio.ViewModels;

public partial class SettingsViewModel : ObservableRecipient
{
    private readonly IAppInfoService _appInfoService;
    private readonly IUserSettings _userSettings;
    private readonly IAppSettingsService _appSettings;
    private readonly IUIDispatcher _dispatcher;
    private readonly IDSCExplorer _dscExplorer;
    private readonly IAppOperationHub _operationHub;
    private readonly IStringLocalizer<SettingsViewModel> _localizer;

    [ObservableProperty]
    public partial int SelectedThemeIndex { get; set; }

    [ObservableProperty]
    public partial string VersionDescription { get; set; }

    [ObservableProperty]
    public partial bool DisableTelemetry { get; set; }

    public List<Theme> Themes { get; }

    public SettingsViewModel(
        IAppInfoService appInfoService,
        IUserSettings userSettings,
        IAppSettingsService appSettings,
        IUIDispatcher dispatcher,
        IDSCExplorer dscExplorer,
        IAppOperationHub operationHub,
        IStringLocalizer<SettingsViewModel> localizer)
    {
        _appInfoService = appInfoService;
        _userSettings = userSettings;
        _appSettings = appSettings;
        _dispatcher = dispatcher;
        _dscExplorer = dscExplorer;
        _operationHub = operationHub;
        _localizer = localizer;

        // Initialize themes
        Themes = [
            new Theme(_localizer["Settings_Theme_Light"], ElementTheme.Light),
            new Theme(_localizer["Settings_Theme_Dark"], ElementTheme.Dark),
            new Theme(_localizer["Settings_Theme_Default"], ElementTheme.Default),
        ];

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
    private async Task SwitchThemeAsync(Theme newTheme)
    {
        var valueChanged = SelectedThemeIndex != Themes.IndexOf(newTheme);
        if (valueChanged)
        {
            await _userSettings.SaveAsync(settings => settings.Theme = newTheme.ElementTheme.ToString());
        }
    }

    [RelayCommand]
    private async Task ToggleTelemetryAsync(bool oldValue)
    {
        var valueChanged = DisableTelemetry == oldValue;
        if (valueChanged)
        {
            await _userSettings.SaveAsync(settings => settings.Telemetry.Disable = !oldValue);
        }
    }

    [RelayCommand]
    private async Task ClearModuleCatalogsCacheAsync()
    {
        await _operationHub.RunWithNotificationAsync(async (context, factory) =>
        {
            await _dscExplorer.ClearCacheAsync();
            context.Success(props => props with { Message = _localizer["ClearDSCResourcesCache_CompletedMessage"] });
        });
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

    [RelayCommand]
    private async Task OnOpenLogsAsync()
    {
        await Launcher.LaunchUriAsync(new Uri(_appInfoService.GetAppInstanceLogPath()));
    }

    [RelayCommand]
    private async Task OnOpenSettingsAsync()
    {
        await Launcher.LaunchUriAsync(new Uri(_userSettings.FullPath));
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
        // Update theme
        var theme = _appSettings.GetFeature<ThemeFeatureSettings>().Theme;
        SelectedThemeIndex = Math.Max(Themes.FindIndex(t => t.ElementTheme == theme), 0);

        // Update telemetry
        DisableTelemetry = _appSettings.GetFeature<TelemetryFeatureSettings>().IsDisabled;
    }
}

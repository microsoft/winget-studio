// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using WinGetStudio.Contracts.Services;
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
        IUIDispatcher dispatcher)
    {
        _appInfoService = appInfoService;
        _userSettings = userSettings;
        _appSettings = appSettings;
        _dispatcher = dispatcher;

        // Initialize settings
        VersionDescription = GetVersionDescription();
        RefreshSettings();
    }

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
    private void OnLoaded()
    {
        _userSettings.SettingsChanged += OnSettingsChanged;
    }

    [RelayCommand]
    private void OnUnloaded()
    {
        _userSettings.SettingsChanged -= OnSettingsChanged;
    }

    private async void OnSettingsChanged(object? sender, GeneralSettings e)
    {
        await _dispatcher.EnqueueAsync(RefreshSettings);
    }

    private void RefreshSettings()
    {
        ElementTheme = _appSettings.GetFeature<ThemeFeatureSettings>().Theme;
        DisableTelemetry = _appSettings.GetFeature<TelemetryFeatureSettings>().IsDisabled;
    }
}

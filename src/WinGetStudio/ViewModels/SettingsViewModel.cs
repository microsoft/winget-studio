// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Services.Settings.Contracts;
using WinGetStudio.Services.Settings.Models;

namespace WinGetStudio.ViewModels;

public partial class SettingsViewModel : ObservableRecipient
{
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly IAppInfoService _appInfoService;
    private readonly IUserSettings _userSettings;
    private readonly IUIDispatcher _dispatcher;
    private readonly ITelemetrySettingsService _telemetrySettingsService;

    [ObservableProperty]
    public partial ElementTheme ElementTheme { get; set; }

    [ObservableProperty]
    public partial string VersionDescription { get; set; }

    [ObservableProperty]
    public partial bool DisableTelemetry { get; set; }

    public SettingsViewModel(
        IThemeSelectorService themeSelectorService,
        IAppInfoService appInfoService,
        IUserSettings userSettings,
        IUIDispatcher dispatcher,
        ITelemetrySettingsService telemetrySettingsService)
    {
        _appInfoService = appInfoService;
        _themeSelectorService = themeSelectorService;
        _userSettings = userSettings;
        _dispatcher = dispatcher;
        _telemetrySettingsService = telemetrySettingsService;

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
        ElementTheme = _themeSelectorService.Theme;
        DisableTelemetry = _telemetrySettingsService.IsDisabled;
    }
}

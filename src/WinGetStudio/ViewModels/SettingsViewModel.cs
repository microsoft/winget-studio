// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Services.Settings.Contracts;

namespace WinGetStudio.ViewModels;

public partial class SettingsViewModel : ObservableRecipient
{
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly IAppInfoService _appInfoService;
    private readonly IUserSettings _userSettings;
    private readonly IUIDispatcher _dispatcher;

    [ObservableProperty]
    public partial ElementTheme ElementTheme { get; set; }

    [ObservableProperty]
    public partial string VersionDescription { get; set; }

    public SettingsViewModel(
        IThemeSelectorService themeSelectorService,
        IAppInfoService appInfoService,
        IUserSettings userSettings,
        IUIDispatcher dispatcher)
    {
        _appInfoService = appInfoService;
        _themeSelectorService = themeSelectorService;
        _userSettings = userSettings;
        _dispatcher = dispatcher;
        ElementTheme = _themeSelectorService.Theme;
        VersionDescription = GetVersionDescription();
    }

    private string GetVersionDescription()
    {
        return $"{_appInfoService.GetAppNameLocalized()} - {_appInfoService.GetAppVersion()}";
    }

    [RelayCommand]
    private async Task SwitchThemeAsync(ElementTheme theme)
    {
        if (ElementTheme != theme)
        {
            ElementTheme = theme;
            await _themeSelectorService.SetThemeAsync(theme);
        }
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

    private async void OnSettingsChanged(object? sender, IGeneralSettings e)
    {
        await _dispatcher.EnqueueAsync(() => ElementTheme = _themeSelectorService.Theme);
    }
}

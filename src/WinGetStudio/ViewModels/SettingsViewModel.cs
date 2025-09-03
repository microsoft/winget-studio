// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Services.Settings.Contracts;

namespace WinGetStudio.ViewModels;

public partial class SettingsViewModel : ObservableRecipient
{
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly IAppInfoService _appInfoService;
    private readonly IUserSettings _userSettings;
    private readonly DispatcherQueue _dispatcherQueue;

    [ObservableProperty]
    public partial ElementTheme ElementTheme { get; set; }

    [ObservableProperty]
    public partial string VersionDescription { get; set; }

    public SettingsViewModel(
        IThemeSelectorService themeSelectorService,
        IAppInfoService appInfoService,
        IUserSettings userSettings)
    {
        _appInfoService = appInfoService;
        _themeSelectorService = themeSelectorService;
        _userSettings = userSettings;
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        ElementTheme = _themeSelectorService.Theme;
        VersionDescription = GetVersionDescription();

        _userSettings.SettingsChanged += async (_, _) =>
        {
            await _dispatcherQueue.EnqueueAsync(async () =>
            {
                await _themeSelectorService.InitializeAsync();
                await SwitchThemeAsync(_themeSelectorService.Theme);
            });
        };
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
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Services.Settings.Contracts;

namespace WinGetStudio.Services;

public class ThemeSelectorService : IThemeSelectorService
{
    private readonly IUserSettings _userSettings;
    private readonly IThemeApplierService _themeApplier;

    public ElementTheme Theme => GetElementTheme(_userSettings.Current.Theme);

    public ThemeSelectorService(IUserSettings userSettings, IThemeApplierService themeApplier)
    {
        _userSettings = userSettings;
        _themeApplier = themeApplier;
        _userSettings.SettingsChanged += OnSettingsChanged;
    }

    public async Task SetThemeAsync(ElementTheme theme)
    {
        if (theme != Theme)
        {
            await _themeApplier.ApplyThemeAsync(Theme);
            await _userSettings.SaveAsync(settings => settings.Theme = theme.ToString());
        }
    }

    public async Task ApplyThemeAsync()
    {
        await _themeApplier.ApplyThemeAsync(Theme);
    }

    private ElementTheme GetElementTheme(string theme)
    {
        if (Enum.TryParse(theme, out ElementTheme cacheTheme))
        {
            return cacheTheme;
        }

        return ElementTheme.Default;
    }

    private async void OnSettingsChanged(object? sender, IGeneralSettings e)
    {
        var theme = GetElementTheme(e.Theme);
        await _themeApplier.ApplyThemeAsync(theme);
    }
}

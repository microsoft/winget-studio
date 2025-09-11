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

    /// <inheritdoc/>
    public ElementTheme Theme => GetElementTheme(_userSettings.Current.Theme);

    public ThemeSelectorService(IUserSettings userSettings, IThemeApplierService themeApplier)
    {
        _userSettings = userSettings;
        _themeApplier = themeApplier;
        _userSettings.SettingsChanged += OnSettingsChanged;
    }

    /// <inheritdoc/>
    public async Task SetThemeAsync(ElementTheme theme)
    {
        if (theme != Theme)
        {
            await _themeApplier.ApplyThemeAsync(Theme);
            await _userSettings.SaveAsync(settings => settings.Theme = theme.ToString());
        }
    }

    /// <inheritdoc/>
    public async Task ApplyThemeAsync()
    {
        await _themeApplier.ApplyThemeAsync(Theme);
    }

    /// <summary>
    /// Converts a string representation of a theme to an ElementTheme.
    /// </summary>
    /// <param name="theme">The string representation of the theme.</param>
    /// <returns>The corresponding ElementTheme, or ElementTheme.Default if the
    /// string is invalid.</returns>
    private ElementTheme GetElementTheme(string theme)
    {
        if (Enum.TryParse(theme, ignoreCase: true, out ElementTheme cacheTheme))
        {
            return cacheTheme;
        }

        return ElementTheme.Default;
    }

    private async void OnSettingsChanged(object? sender, IGeneralSettings newSettings)
    {
        var theme = GetElementTheme(newSettings.Theme);
        await _themeApplier.ApplyThemeAsync(theme);
    }
}

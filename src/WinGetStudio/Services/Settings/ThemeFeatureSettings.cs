// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Services.Settings.Contracts;
using WinGetStudio.Services.Settings.Models;

namespace WinGetStudio.Services.Settings;

public class ThemeFeatureSettings : IFeatureSettingsService
{
    private readonly IUserSettings _userSettings;
    private readonly IThemeApplierService _themeApplier;

    /// <summary>
    /// Event that is raised when the theme is applied.
    /// </summary>
    public event EventHandler<ElementTheme>? ThemeApplied;

    /// <summary>
    /// Gets the current theme.
    /// </summary>
    public ElementTheme Theme => GetElementTheme(_userSettings.Current.Theme);

    public ApplicationTheme DefaultAppTheme => App.Current.RequestedTheme;

    public ThemeFeatureSettings(IUserSettings userSettings, IThemeApplierService themeApplier)
    {
        _userSettings = userSettings;
        _themeApplier = themeApplier;

        _userSettings.SettingsChanged += OnSettingsChanged;
    }

    /// <inheritdoc/>
    public async Task ApplySettingsAsync()
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

    private async void OnSettingsChanged(object? sender, GeneralSettings newSettings)
    {
        var theme = GetElementTheme(newSettings.Theme);
        await _themeApplier.ApplyThemeAsync(theme);
        ThemeApplied?.Invoke(this, theme);
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;

namespace WinGetStudio.Contracts.Services;

public interface IThemeSelectorService
{
    ElementTheme Theme { get; }

    Task SetThemeAsync(ElementTheme theme);

    Task ApplyThemeAsync();
}

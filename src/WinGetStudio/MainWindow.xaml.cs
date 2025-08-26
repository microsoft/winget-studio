// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.UI.Dispatching;
using Windows.UI.ViewManagement;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Helpers;

namespace WinGetStudio;

public sealed partial class MainWindow : WindowEx
{
    private readonly DispatcherQueue _dispatcherQueue;

    private readonly UISettings _settings;

    public MainWindow()
    {
        InitializeComponent();

        var appInfoService = App.GetService<IAppInfoService>();
        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/WindowIcon.ico"));
        Content = null;
        Title = appInfoService.GetAppNameLocalized();

        // Theme change code picked from https://github.com/microsoft/WinUI-Gallery/pull/1239
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        _settings = new UISettings();
        _settings.ColorValuesChanged += Settings_ColorValuesChanged; // cannot use FrameworkElement.ActualThemeChanged event
    }

    // this handles updating the caption button colors correctly when indows system theme is changed
    // while the app is open
    private void Settings_ColorValuesChanged(UISettings sender, object args)
    {
        // This calls comes off-thread, hence we will need to dispatch it to current app's thread
        _dispatcherQueue.TryEnqueue(() =>
        {
            TitleBarHelper.ApplySystemThemeToCaptionButtons();
        });
    }
}

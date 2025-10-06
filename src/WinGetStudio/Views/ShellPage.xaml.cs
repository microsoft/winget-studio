// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.System;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Contracts.Views;
using WinGetStudio.Helpers;
using WinGetStudio.Services.Settings.Contracts;
using WingetStudio.Services.VisualFeedback.Contracts;
using WingetStudio.Services.VisualFeedback.Models;
using WinGetStudio.ViewModels;

namespace WinGetStudio.Views;

public sealed partial class ShellPage : Page, IView<ShellViewModel>
{
    private readonly IAppInfoService _appInfoService;
    private readonly IUIFeedbackService _uiFeedbackService;
    private readonly IUserSettings _userSettings;

    public ShellViewModel ViewModel { get; }

    public ShellPage(ShellViewModel viewModel)
    {
        _appInfoService = App.GetService<IAppInfoService>();
        _uiFeedbackService = App.GetService<IUIFeedbackService>();
        _userSettings = App.GetService<IUserSettings>();
        ViewModel = viewModel;
        InitializeComponent();

        ViewModel.NavigationService.Frame = NavigationFrame;
        ViewModel.NavigationViewService.Initialize(NavigationViewControl);

        // TODO: Set the title bar icon by updating /Assets/WindowIcon.ico.
        // A custom title bar is required for full window theme and Mica support.
        // https://docs.microsoft.com/windows/apps/develop/title-bar?tabs=winui3#full-customization
        App.MainWindow.ExtendsContentIntoTitleBar = true;
        App.MainWindow.SetTitleBar(AppTitleBar);
        App.MainWindow.Activated += MainWindow_Activated;
        AppTitleBarText.Text = _appInfoService.GetAppNameLocalized();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        TitleBarHelper.UpdateTitleBar(RequestedTheme);

        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.Left, VirtualKeyModifiers.Menu));
        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.GoBack));

        _uiFeedbackService.Notification.NotificationShown += OnNotificationShown;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _uiFeedbackService.Notification.NotificationShown -= OnNotificationShown;
    }

    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        App.AppTitlebar = AppTitleBarText as UIElement;
    }

    private void NavigationViewControl_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
    {
        AppTitleBar.Margin = new Thickness()
        {
            Left = sender.CompactPaneLength * (sender.DisplayMode == NavigationViewDisplayMode.Minimal ? 2 : 1),
            Top = AppTitleBar.Margin.Top,
            Right = AppTitleBar.Margin.Right,
            Bottom = AppTitleBar.Margin.Bottom,
        };

        SplitViewPaneHeader.Background = sender.DisplayMode == NavigationViewDisplayMode.Minimal ? SplitViewPaneContent.Background : null;
    }

    private static KeyboardAccelerator BuildKeyboardAccelerator(VirtualKey key, VirtualKeyModifiers? modifiers = null)
    {
        var keyboardAccelerator = new KeyboardAccelerator() { Key = key };

        if (modifiers.HasValue)
        {
            keyboardAccelerator.Modifiers = modifiers.Value;
        }

        keyboardAccelerator.Invoked += OnKeyboardAcceleratorInvoked;

        return keyboardAccelerator;
    }

    private static void OnKeyboardAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        var navigationService = App.GetService<IAppFrameNavigationService>();

        var result = navigationService.GoBack();

        args.Handled = result;
    }

    private void OnNotificationShown(object? sender, NotificationMessage message)
    {
        if (message.ShownBehavior == NotificationShownBehavior.ClearOverlays)
        {
            NotificationQueue.Clear();
        }

        if (message.Delivery.HasFlag(NotificationDelivery.Overlay))
        {
            TimeSpan? duration = null;
            if (message.DismissBehavior == NotificationDismissBehavior.Timeout && message.Duration > TimeSpan.Zero)
            {
                duration = message.Duration;
            }

            NotificationQueue.Show(new()
            {
                Title = message.Title,
                Message = message.Message,
                Severity = NotificationHelper.GetInfoBarSeverity(message.Severity),
                Content = message,
                ContentTemplate = new DataTemplate(),
                Duration = duration,
            });
        }
    }

    private void NotificationRead(InfoBar sender, object args)
    {
        if (sender?.Content is NotificationMessage message)
        {
            ViewModel.MarkAsRead(message);
        }
    }
}

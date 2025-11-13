// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.System;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Contracts.Views;
using WinGetStudio.Helpers;
using WinGetStudio.Services.Operations.Contracts;
using WinGetStudio.Services.Operations.Models.States;
using WinGetStudio.ViewModels;

namespace WinGetStudio.Views;

public sealed partial class ShellPage : Page, IView<ShellViewModel>
{
    private const int MaxNotificationMessageLength = 512;
    private readonly IAppInfoService _appInfoService;
    private readonly IOperationHub _operationHub;

    public ShellViewModel ViewModel { get; }

    public ShellPage(ShellViewModel viewModel)
    {
        _appInfoService = App.GetService<IAppInfoService>();
        _operationHub = App.GetService<IOperationHub>();
        _operationHub.Notifications.Subscribe(OnPublishNotification);
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

    private void OnPublishNotification(OperationNotification notification)
    {
        NotificationQueue.Clear();

        // Limit message length to avoid UI errors.
        var title = notification.Properties.Title ?? string.Empty;
        if (title.Length > MaxNotificationMessageLength)
        {
            title = string.Concat(title.AsSpan(0, MaxNotificationMessageLength), "…");
        }

        var message = notification.Properties.Message ?? string.Empty;
        if (message.Length > MaxNotificationMessageLength)
        {
            message = string.Concat(message.AsSpan(0, MaxNotificationMessageLength), "…");
        }

        NotificationQueue.Show(new()
        {
            Title = title,
            Message = message,
            Severity = ActivityHelper.GetInfoBarSeverity(notification.Properties.Severity),
            Content = message,
            ContentTemplate = new DataTemplate(),
            Duration = notification.Duration,
        });
    }
}

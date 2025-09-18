// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Navigation;
using WinGetStudio.Contracts.Services;
using WingetStudio.Services.VisualFeedback.Contracts;
using WingetStudio.Services.VisualFeedback.Models;
using WinGetStudio.Views;

namespace WinGetStudio.ViewModels;

public partial class ShellViewModel : ObservableRecipient
{
    private readonly IUIFeedbackService _uiFeedbackService;
    private readonly DispatcherQueue _dispatcherQueue;

    [ObservableProperty]
    public partial bool IsBackEnabled { get; set; }

    [ObservableProperty]
    public partial object? Selected { get; set; }

    [ObservableProperty]
    public partial bool IsNotificationPaneOpen { get; set; }

    [ObservableProperty]
    public partial int UnreadNotificationsCount { get; set; }

    public IAppNavigationService NavigationService { get; }

    public INavigationViewService NavigationViewService { get; }

    public ShellViewModel(
        IAppNavigationService navigationService,
        INavigationViewService navigationViewService,
        IUIFeedbackService uiFeedbackService)
    {
        NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;
        NavigationViewService = navigationViewService;
        _uiFeedbackService = uiFeedbackService;
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        IsBackEnabled = NavigationService.CanGoBack;

        if (e.SourcePageType == typeof(SettingsPage))
        {
            Selected = NavigationViewService.SettingsItem;
            return;
        }

        var selectedItem = NavigationViewService.GetSelectedItem(e.SourcePageType);
        if (selectedItem != null)
        {
            Selected = selectedItem;
        }
    }

    [RelayCommand]
    private void OnLoaded()
    {
        _uiFeedbackService.Notification.NotificationRead += OnNotificationRead;
        _uiFeedbackService.Notification.NotificationShown += OnNotificationShown;
    }

    [RelayCommand]
    private void OnUnloaded()
    {
        _uiFeedbackService.Notification.NotificationRead -= OnNotificationRead;
        _uiFeedbackService.Notification.NotificationShown -= OnNotificationShown;
    }

    [RelayCommand]
    private void OnToggleNotificationPane()
    {
        IsNotificationPaneOpen = !IsNotificationPaneOpen;
    }

    public void MarkAsRead(NotificationMessage message)
    {
        _uiFeedbackService.Notification.MarkAsRead(message);
    }

    private async void OnNotificationRead(object? sender, NotificationMessage message)
    {
        await UpdateUnreadNotificationsCount();
    }

    private async void OnNotificationShown(object? sender, NotificationMessage message)
    {
        await UpdateUnreadNotificationsCount();
    }

    private async Task UpdateUnreadNotificationsCount()
    {
        await _dispatcherQueue.EnqueueAsync(() =>
        {
            UnreadNotificationsCount = _uiFeedbackService.Notification.UnreadCount;
        });
    }
}

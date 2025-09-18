// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using Microsoft.UI.Dispatching;
using WingetStudio.Services.VisualFeedback.Contracts;
using WingetStudio.Services.VisualFeedback.Models;

namespace WinGetStudio.ViewModels.Controls;

public partial class NotificationPaneViewModel : ObservableRecipient
{
    private readonly IUIFeedbackService _uiFeedbackService;
    private readonly DispatcherQueue _dispatcherQueue;

    [ObservableProperty]
    public partial bool ArePreviousNotificationsVisible { get; set; }

    public ObservableCollection<NotificationMessage> UnreadNotifications { get; }

    public ObservableCollection<NotificationMessage> PreviousNotifications { get; }

    public NotificationPaneViewModel(IUIFeedbackService uiFeedbackService)
    {
        _uiFeedbackService = uiFeedbackService;
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        UnreadNotifications = new(_uiFeedbackService.Notification.UnreadNotifications);
        PreviousNotifications = new(_uiFeedbackService.Notification.ReadNotifications);
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

    private async void OnNotificationShown(object? sender, NotificationMessage message)
    {
        await _dispatcherQueue.EnqueueAsync(() =>
        {
            if (message.Delivery.HasFlag(NotificationDelivery.Panel))
            {
                UnreadNotifications.Insert(0, message);
            }
        });
    }

    private async void OnNotificationRead(object? sender, NotificationMessage message)
    {
        await _dispatcherQueue.EnqueueAsync(() =>
        {
            Debug.Assert(message.Delivery.HasFlag(NotificationDelivery.Panel), "Only panel notifications should raise the read event.");
            UnreadNotifications.Remove(message);
            PreviousNotifications.Insert(0, message);
        });
    }

    [RelayCommand]
    private void OnShowPreviousNotifications()
    {
        ArePreviousNotificationsVisible = true;
    }

    [RelayCommand]
    private void OnMarkAsRead(NotificationMessage message)
    {
        _uiFeedbackService.Notification.MarkAsRead(message);
    }
}

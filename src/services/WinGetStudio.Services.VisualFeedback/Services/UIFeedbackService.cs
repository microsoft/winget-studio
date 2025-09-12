// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using WingetStudio.Services.VisualFeedback.Contracts;
using WingetStudio.Services.VisualFeedback.Models;

namespace WingetStudio.Services.VisualFeedback.Services;

internal sealed class UIFeedbackService : IUIFeedbackService
{
    /// <inheritdoc/>
    public ILoadingService Loading { get; }

    /// <inheritdoc/>
    public INotificationService Notification { get; }

    public UIFeedbackService(ILoadingService loadingService, INotificationService notificationService)
    {
        Loading = loadingService;
        Notification = notificationService;
    }

    /// <inheritdoc/>
    public void ShowOutcomeNotification(string title, string message, NotificationMessageSeverity severity)
    {
        Notification.Show(new()
        {
            Title = title,
            Message = message,
            Severity = severity,
            Delivery = NotificationDelivery.Overlay,
            DismissBehavior = NotificationDismissBehavior.User,
            ShownBehavior = NotificationShownBehavior.ClearOverlays,
        });
    }

    /// <inheritdoc/>
    public void ClearOverlayNotifications()
    {
        Notification.Show(new()
        {
            Delivery = NotificationDelivery.None,
            ShownBehavior = NotificationShownBehavior.ClearOverlays,
        });
    }

    /// <inheritdoc/>
    public void ShowTaskProgress(int progressValue = 0)
    {
        if (Math.Max(progressValue, 0) == 0)
        {
            Loading.SetIndeterminate(true);
        }
        else
        {
            Loading.SetProgressValue(progressValue);
        }

        Loading.SetVisibility(true);
    }

    /// <inheritdoc/>
    public void HideTaskProgress()
    {
        Loading.SetVisibility(false);
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using WingetStudio.Services.VisualFeedback.Models;

namespace WingetStudio.Services.VisualFeedback.Contracts;

public interface IUIFeedbackService
{
    /// <summary>
    /// Gets the loading service.
    /// </summary>
    ILoadingService Loading { get; }

    /// <summary>
    /// Gets the notification service.
    /// </summary>
    INotificationService Notification { get; }

    /// <summary>
    /// Shows an outcome notification.
    /// </summary>
    /// <param name="title">The title of the notification.</param>
    /// <param name="message">The message of the notification.</param>
    /// <param name="severity">The severity of the notification.</param>
    void ShowOutcomeNotification(string title, string message, NotificationMessageSeverity severity);

    /// <summary>
    /// Clears all overlay notifications.
    /// </summary>
    void ClearOverlayNotifications();

    /// <summary>
    /// Shows task progress.
    /// </summary>
    /// <param name="progressValue">The progress value (0-100). Default is 0 (indeterminate).</param>
    void ShowTaskProgress(int progressValue = 0);

    /// <summary>
    /// Hides task progress.
    /// </summary>
    void HideTaskProgress();
}

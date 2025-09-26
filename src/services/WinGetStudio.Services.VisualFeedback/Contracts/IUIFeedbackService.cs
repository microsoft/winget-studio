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
    /// Shows a timed notification.
    /// </summary>
    /// <param name="message">Optional message.</param>
    /// <param name="severity">Severity of the message.</param>
    /// <param name="duration">Duration to show the message.</param>
    void ShowTimedNotification(string message, NotificationMessageSeverity severity, TimeSpan duration = default);

    /// <summary>
    /// Shows a timed notification.
    /// </summary>
    /// <param name="title">Optional title.</param>
    /// <param name="message">Optional message.</param>
    /// <param name="severity">Severity of the message.</param>
    /// <param name="duration">Duration to show the message.</param>
    void ShowTimedNotification(string title, string message, NotificationMessageSeverity severity, TimeSpan duration = default);

    /// <summary>
    /// Shows a persistent notification.
    /// </summary>
    /// <param name="message">Optional message.</param>
    /// <param name="severity">Severity of the message.</param>
    void ShowPersistentNotification(string message, NotificationMessageSeverity severity);

    /// <summary>
    /// Shows a persistent notification.
    /// </summary>
    /// <param name="title">Optional title.</param>
    /// <param name="message">Optional message.</param>
    /// <param name="severity">Severity of the message.</param>
    void ShowPersistentNotification(string title, string message, NotificationMessageSeverity severity);

    /// <summary>
    /// Clears all notifications.
    /// </summary>
    void ClearNotifications();

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

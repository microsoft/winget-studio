// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using WingetStudio.Services.VisualFeedback.Models;

namespace WingetStudio.Services.VisualFeedback.Contracts;

public interface INotificationService
{
    /// <summary>
    /// Event raised when a notification is shown.
    /// </summary>
    event EventHandler<NotificationMessage> NotificationShown;

    /// <summary>
    /// Event raised when a notification is marked as read.
    /// </summary>
    event EventHandler<NotificationMessage> NotificationRead;

    /// <summary>
    /// Gets all notifications, both read and unread.
    /// </summary>
    IReadOnlyCollection<NotificationMessage> AllNotifications { get; }

    /// <summary>
    /// Gets unread notifications only.
    /// </summary>
    IReadOnlyCollection<NotificationMessage> UnreadNotifications { get; }

    /// <summary>
    /// Gets read notifications only.
    /// </summary>
    IReadOnlyCollection<NotificationMessage> ReadNotifications { get; }

    /// <summary>
    /// Gets the count of unread notifications.
    /// </summary>
    int UnreadCount { get; }

    /// <summary>
    /// Shows a notification message.
    /// </summary>
    /// <param name="message">The message to show.</param>
    void Show(NotificationMessage message);

    /// <summary>
    /// Gets whether a notification has been read.
    /// </summary>
    /// <param name="message">The message to check.</param>
    /// <returns>True if the message has been read, false otherwise.</returns>
    bool IsRead(NotificationMessage message);

    /// <summary>
    /// Marks the specified notification message as read.
    /// </summary>
    /// <param name="message">The notification message to mark as read.</param>
    /// <returns>True if the message was successfully marked as read, false otherwise.</returns>
    bool MarkAsRead(NotificationMessage message);
}

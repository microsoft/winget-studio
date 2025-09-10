// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using WingetStudio.Services.VisualFeedback.Models;

namespace WingetStudio.Services.VisualFeedback.Contracts;

public interface INotificationService
{
    event EventHandler<NotificationMessage> NotificationShown;

    event EventHandler<NotificationMessage> NotificationRead;

    IReadOnlyCollection<NotificationMessage> AllNotifications { get; }

    IReadOnlyCollection<NotificationMessage> UnreadNotifications { get; }

    IReadOnlyCollection<NotificationMessage> ReadNotifications { get; }

    int UnreadCount { get; }

    void Show(NotificationMessage message);

    bool IsRead(NotificationMessage message);

    bool MarkAsRead(NotificationMessage message);
}

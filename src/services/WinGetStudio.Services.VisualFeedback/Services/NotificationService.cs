// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using WingetStudio.Services.VisualFeedback.Contracts;
using WingetStudio.Services.VisualFeedback.Models;

namespace WingetStudio.Services.VisualFeedback.Services;

internal sealed class NotificationService : INotificationService
{
    private readonly object _lock = new();
    private readonly List<NotificationEntry> _panelNotification = [];

    /// <inheritdoc/>
    public event EventHandler<NotificationMessage> NotificationShown;

    /// <inheritdoc/>
    public event EventHandler<NotificationMessage> NotificationRead;

    /// <inheritdoc/>
    public IReadOnlyCollection<NotificationMessage> AllNotifications => [.. _panelNotification.Select(n => n.Message)];

    /// <inheritdoc/>
    public IReadOnlyCollection<NotificationMessage> UnreadNotifications => [.. _panelNotification.Where(n => !n.IsRead).Select(n => n.Message)];

    /// <inheritdoc/>
    public IReadOnlyCollection<NotificationMessage> ReadNotifications => [.. _panelNotification.Where(n => n.IsRead).Select(n => n.Message)];

    /// <inheritdoc/>
    public int UnreadCount => _panelNotification.Count(n => !n.IsRead);

    /// <inheritdoc/>
    public void Show(NotificationMessage message)
    {
        // Only panel notifications are tracked for read/unread state.
        if (message.Delivery.HasFlag(NotificationDelivery.Panel))
        {
            lock (_lock)
            {
                _panelNotification.Add(new()
                {
                    Message = message,
                    IsRead = false,
                });
            }
        }

        NotificationShown?.Invoke(this, message);
    }

    /// <inheritdoc/>
    public bool IsRead(NotificationMessage message)
    {
        lock (_lock)
        {
            var entry = _panelNotification.FirstOrDefault(n => n.Message == message);
            return entry is not null && entry.IsRead;
        }
    }

    /// <inheritdoc/>
    public bool MarkAsRead(NotificationMessage message)
    {
        EventHandler<NotificationMessage> handler = null;
        lock (_lock)
        {
            var entry = _panelNotification.FirstOrDefault(n => n.Message == message);
            if (entry is not null)
            {
                entry.IsRead = true;
                handler = NotificationRead;
            }
        }

        handler?.Invoke(this, message);
        return handler != null;
    }
}

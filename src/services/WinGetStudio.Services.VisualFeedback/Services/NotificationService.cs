// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using WingetStudio.Services.VisualFeedback.Contracts;
using WingetStudio.Services.VisualFeedback.Models;

namespace WingetStudio.Services.VisualFeedback.Services;

internal sealed class NotificationService : INotificationService
{
    private readonly ConcurrentStack<NotificationEntry> _notification = [];

    public event EventHandler<NotificationMessage> NotificationShown;

    public event EventHandler<NotificationMessage> NotificationRead;

    public IReadOnlyCollection<NotificationMessage> AllNotifications => [.. _notification.Select(n => n.Message)];

    public IReadOnlyCollection<NotificationMessage> UnreadNotifications => [.. _notification.Where(n => !n.IsRead).Select(n => n.Message)];

    public IReadOnlyCollection<NotificationMessage> ReadNotifications => [.. _notification.Where(n => n.IsRead).Select(n => n.Message)];

    public int UnreadCount => _notification.Count(n => !n.IsRead);

    public void Show(NotificationMessage message)
    {
        _notification.Push(new()
        {
            Message = message,
            IsRead = false,
        });
        NotificationShown?.Invoke(this, message);
    }

    public bool IsRead(NotificationMessage message)
    {
        var entry = _notification.FirstOrDefault(n => n.Message == message);
        return entry is not null && entry.IsRead;
    }

    public bool MarkAsRead(NotificationMessage message)
    {
        var entry = _notification.FirstOrDefault(n => n.Message == message);
        if (entry is not null)
        {
            entry.IsRead = true;
            NotificationRead?.Invoke(this, message);
            return true;
        }

        return false;
    }
}

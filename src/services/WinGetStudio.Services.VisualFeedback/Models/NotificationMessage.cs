// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace WingetStudio.Services.VisualFeedback.Models;

public sealed class NotificationMessage
{
    /// <summary>
    /// Gets or sets the title of the notification.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the message of the notification.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets the duration the notification is displayed before auto-dismissing.
    /// </summary>
    public TimeSpan Duration { get; set; } = TimeSpan.FromSeconds(3);

    /// <summary>
    /// Gets or sets the severity of the notification.
    /// </summary>
    public NotificationMessageSeverity Severity { get; set; } = NotificationMessageSeverity.Informational;

    /// <summary>
    /// Gets or sets the delivery method of the notification.
    /// </summary>
    public NotificationDelivery Delivery { get; set; } = NotificationDelivery.None;

    /// <summary>
    /// Gets or sets the dismiss behavior of the notification.
    /// </summary>
    public NotificationDismissBehavior DismissBehavior { get; set; } = NotificationDismissBehavior.Timeout;

    /// <summary>
    /// Gets or sets the shown behavior of the notification.
    /// </summary>
    public NotificationShownBehavior ShownBehavior { get; set; } = NotificationShownBehavior.None;
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
    /// Gets or sets the type of the notification.
    /// </summary>
    public NotificationMessageType Type { get; set; } = NotificationMessageType.Informational;
}

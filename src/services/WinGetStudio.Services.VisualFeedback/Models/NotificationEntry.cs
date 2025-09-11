// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WingetStudio.Services.VisualFeedback.Models;

internal sealed class NotificationEntry
{
    /// <summary>
    /// Gets the notification message.
    /// </summary>
    public NotificationMessage Message { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the notification has been read.
    /// </summary>
    public bool IsRead { get; set; }
}

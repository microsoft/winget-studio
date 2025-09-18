// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WingetStudio.Services.VisualFeedback.Models;

public enum NotificationDismissBehavior
{
    /// <summary>
    /// The notification will be dismissed automatically after the specified duration.
    /// </summary>
    /// <remarks>If the duration is zero, the notification will not auto-dismiss.</remarks>
    Timeout,

    /// <summary>
    /// The notification will be dismissed when the user interacts with it.
    /// </summary>
    User,
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WingetStudio.Services.VisualFeedback.Models;

public enum NotificationShownBehavior
{
    /// <summary>
    /// No special behavior when the notification is shown.
    /// </summary>
    None,

    /// <summary>
    /// Clear all existing overlay notifications when this notification is shown.
    /// </summary>
    ClearOverlays,
}

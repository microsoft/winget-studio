// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace WingetStudio.Services.VisualFeedback.Models;

[Flags]
public enum NotificationDelivery
{
    /// <summary>
    /// No notification delivery.
    /// </summary>
    None = 0,

    /// <summary>
    /// Notification delivered to panel.
    /// </summary>
    Panel = 1 << 0,

    /// <summary>
    /// Notification delivered as overlay.
    /// </summary>
    Overlay = 1 << 1,
}

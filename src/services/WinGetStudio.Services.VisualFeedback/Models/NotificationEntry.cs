// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WingetStudio.Services.VisualFeedback.Models;

internal sealed class NotificationEntry
{
    public NotificationMessage Message { get; init; }

    public bool IsRead { get; set; }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WingetStudio.Services.VisualFeedback.Models;

public sealed class NotificationMessage
{
    public string Title { get; set; }

    public string Message { get; set; }

    public NotificationMessageType Type { get; set; } = NotificationMessageType.Informational;
}

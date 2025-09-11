// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WingetStudio.Services.VisualFeedback.Contracts;

public interface IUIFeedbackService
{
    /// <summary>
    /// Gets the loading service.
    /// </summary>
    ILoadingService Loading { get; }

    /// <summary>
    /// Gets the notification service.
    /// </summary>
    INotificationService Notification { get; }
}

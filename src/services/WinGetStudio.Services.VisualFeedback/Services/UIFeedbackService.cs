// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WingetStudio.Services.VisualFeedback.Contracts;

namespace WingetStudio.Services.VisualFeedback.Services;

internal sealed class UIFeedbackService : IUIFeedbackService
{
    public ILoadingService Loading { get; }

    public INotificationService Notification { get; }

    public UIFeedbackService(ILoadingService loadingService, INotificationService notificationService)
    {
        Loading = loadingService;
        Notification = notificationService;
    }
}

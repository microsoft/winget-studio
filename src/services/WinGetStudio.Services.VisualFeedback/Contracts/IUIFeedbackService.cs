// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WingetStudio.Services.VisualFeedback.Contracts;

public interface IUIFeedbackService
{
    ILoadingService Loading { get; }

    INotificationService Notification { get; }
}

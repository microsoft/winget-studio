// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using WinGetStudio.Services.Core.Extensions;
using WingetStudio.Services.VisualFeedback.Contracts;
using WingetStudio.Services.VisualFeedback.Services;

namespace WinGetStudio.Services.VisualFeedback.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddVisualFeedback(this IServiceCollection services)
    {
        services.AddCore();
        services.AddSingleton<ILoadingService, LoadingService>();
        services.AddSingleton<INotificationService, NotificationService>();
        services.AddSingleton<IUIFeedbackService, UIFeedbackService>();
        return services;
    }
}

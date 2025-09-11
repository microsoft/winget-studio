// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.UI.Xaml.Controls;
using WingetStudio.Services.VisualFeedback.Models;

namespace WinGetStudio.Helpers;

public static class NotificationHelper
{
    public static NotificationMessageType GetNotificationMessageType(InfoBarSeverity severity)
    {
        return severity switch
        {
            InfoBarSeverity.Informational => NotificationMessageType.Informational,
            InfoBarSeverity.Warning => NotificationMessageType.Warning,
            InfoBarSeverity.Error => NotificationMessageType.Error,
            InfoBarSeverity.Success => NotificationMessageType.Success,
            _ => NotificationMessageType.Informational,
        };
    }

    public static InfoBarSeverity GetInfoBarSeverity(NotificationMessageType type)
    {
        return type switch
        {
            NotificationMessageType.Informational => InfoBarSeverity.Informational,
            NotificationMessageType.Warning => InfoBarSeverity.Warning,
            NotificationMessageType.Error => InfoBarSeverity.Error,
            NotificationMessageType.Success => InfoBarSeverity.Success,
            _ => InfoBarSeverity.Informational,
        };
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.UI.Xaml.Controls;
using WingetStudio.Services.VisualFeedback.Models;

namespace WinGetStudio.Helpers;

public static class NotificationHelper
{
    public static NotificationMessageSeverity GetNotificationMessageSeverity(InfoBarSeverity severity)
    {
        return severity switch
        {
            InfoBarSeverity.Informational => NotificationMessageSeverity.Informational,
            InfoBarSeverity.Warning => NotificationMessageSeverity.Warning,
            InfoBarSeverity.Error => NotificationMessageSeverity.Error,
            InfoBarSeverity.Success => NotificationMessageSeverity.Success,
            _ => NotificationMessageSeverity.Informational,
        };
    }

    public static InfoBarSeverity GetInfoBarSeverity(NotificationMessageSeverity severity)
    {
        return severity switch
        {
            NotificationMessageSeverity.Informational => InfoBarSeverity.Informational,
            NotificationMessageSeverity.Warning => InfoBarSeverity.Warning,
            NotificationMessageSeverity.Error => InfoBarSeverity.Error,
            NotificationMessageSeverity.Success => InfoBarSeverity.Success,
            _ => InfoBarSeverity.Informational,
        };
    }
}

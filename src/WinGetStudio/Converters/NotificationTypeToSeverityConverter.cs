// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using WinGetStudio.Helpers;
using WingetStudio.Services.VisualFeedback.Models;

namespace WinGetStudio.Converters;

public partial class NotificationTypeToSeverityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return NotificationHelper.GetInfoBarSeverity((NotificationMessageType)value);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return NotificationHelper.GetNotificationMessageType((InfoBarSeverity)value);
    }
}

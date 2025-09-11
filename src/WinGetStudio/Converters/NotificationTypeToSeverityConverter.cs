// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using WingetStudio.Services.VisualFeedback.Models;

namespace WinGetStudio.Converters;

public partial class NotificationTypeToSeverityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value switch
        {
            NotificationMessageType.Informational => InfoBarSeverity.Informational,
            NotificationMessageType.Warning => InfoBarSeverity.Warning,
            NotificationMessageType.Error => InfoBarSeverity.Error,
            NotificationMessageType.Success => InfoBarSeverity.Success,
            _ => InfoBarSeverity.Informational,
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value switch
        {
            InfoBarSeverity.Informational => NotificationMessageType.Informational,
            InfoBarSeverity.Warning => NotificationMessageType.Warning,
            InfoBarSeverity.Error => NotificationMessageType.Error,
            InfoBarSeverity.Success => NotificationMessageType.Success,
            _ => NotificationMessageType.Informational,
        };
    }
}

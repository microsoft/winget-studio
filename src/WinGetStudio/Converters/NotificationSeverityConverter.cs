// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using WinGetStudio.Helpers;
using WinGetStudio.Services.Operations.Models.States;

namespace WinGetStudio.Converters;

public partial class ActivitySeverityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return ActivityHelper.GetInfoBarSeverity((OperationSeverity)value);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return ActivityHelper.GetActivitySeverity((InfoBarSeverity)value);
    }
}

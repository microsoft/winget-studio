// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.UI.Xaml.Controls;
using WinGetStudio.Services.Operations.Models.State;

namespace WinGetStudio.Helpers;

public static class ActivityHelper
{
    public static OperationSeverity GetActivitySeverity(InfoBarSeverity severity)
    {
        return severity switch
        {
            InfoBarSeverity.Informational => OperationSeverity.Info,
            InfoBarSeverity.Warning => OperationSeverity.Warning,
            InfoBarSeverity.Error => OperationSeverity.Error,
            InfoBarSeverity.Success => OperationSeverity.Success,
            _ => OperationSeverity.Info,
        };
    }

    public static InfoBarSeverity GetInfoBarSeverity(OperationSeverity severity)
    {
        return severity switch
        {
            OperationSeverity.Info => InfoBarSeverity.Informational,
            OperationSeverity.Warning => InfoBarSeverity.Warning,
            OperationSeverity.Error => InfoBarSeverity.Error,
            OperationSeverity.Success => InfoBarSeverity.Success,
            _ => InfoBarSeverity.Informational,
        };
    }
}

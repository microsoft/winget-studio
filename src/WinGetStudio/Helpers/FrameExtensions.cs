// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.UI.Xaml.Controls;
using WinGetStudio.Contracts.Views;

namespace WinGetStudio.Helpers;

public static class FrameExtensions
{
    public static object? GetPageViewModel(this Frame frame) => frame?.Content?.GetType().GetProperty(nameof(IView<object>.ViewModel))?.GetValue(frame.Content, null);
}

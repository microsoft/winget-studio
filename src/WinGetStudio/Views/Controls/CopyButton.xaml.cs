// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace WinGetStudio.Views.Controls;

// This file was copied from the WinUI Gallery project.
// Original file: https://github.com/microsoft/WinUI-Gallery/blob/main/WinUIGallery/Controls/CopyButton.xaml.cs
public sealed partial class CopyButton : Button
{
    public CopyButton()
    {
        DefaultStyleKey = typeof(CopyButton);
    }

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        if (GetTemplateChild("CopyToClipboardSuccessAnimation") is Storyboard storyBoard)
        {
            storyBoard.Begin();
        }
    }

    protected override void OnApplyTemplate()
    {
        Click -= CopyButton_Click;
        base.OnApplyTemplate();
        Click += CopyButton_Click;
    }
}

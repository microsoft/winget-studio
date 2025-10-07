// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.UI.Xaml.Controls;
using WinGetStudio.Contracts.Views;
using WinGetStudio.ViewModels;

namespace WinGetStudio.Views;

public sealed partial class ExportPage : Page, IView<ExportViewModel>
{
    public ExportViewModel ViewModel { get; }

    public ExportPage()
    {
        ViewModel = App.GetService<ExportViewModel>();
        InitializeComponent();
    }
}

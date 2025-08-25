// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.UI.Xaml.Controls;
using WinGetStudio.Contracts.Views;
using WinGetStudio.ViewModels.ConfigurationFlow;

namespace WinGetStudio.Views.ConfigurationFlow;

public sealed partial class ApplyFilePage : Page, IView<ApplyFileViewModel>
{
    public ApplyFileViewModel ViewModel { get; }

    public ApplyFilePage()
    {
        ViewModel = App.GetService<ApplyFileViewModel>();
        InitializeComponent();
    }
}

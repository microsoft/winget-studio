// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.Contracts.Views;
using WinGetStudio.ViewModels.ConfigurationFlow;
using Microsoft.UI.Xaml.Controls;

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

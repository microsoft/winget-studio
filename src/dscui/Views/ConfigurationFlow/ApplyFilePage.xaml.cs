// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using dscui.Contracts.Views;
using dscui.ViewModels.ConfigurationFlow;
using Microsoft.UI.Xaml.Controls;

namespace dscui.Views.ConfigurationFlow;

public sealed partial class ApplyFilePage : Page, IView<ApplyFileViewModel>
{
    public ApplyFileViewModel ViewModel { get; }

    public ApplyFilePage()
    {
        ViewModel = App.GetService<ApplyFileViewModel>();
        InitializeComponent();
    }
}

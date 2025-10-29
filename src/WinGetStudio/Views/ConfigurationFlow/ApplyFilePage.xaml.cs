// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using WinGetStudio.Contracts.Views;
using WinGetStudio.Models;
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

    private void CopyOutputMessage(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is Button copyButton && copyButton.Tag is ApplyUnitViewModel unit)
        {
            var outputText = new StringBuilder();

            if (!string.IsNullOrEmpty(unit.Message))
            {
                outputText.AppendLine(unit.Message);
            }

            if (!string.IsNullOrEmpty(unit.Description))
            {
                outputText.AppendLine(unit.Description);
            }

            var dataPackage = new DataPackage();
            dataPackage.SetText(outputText.ToString());
            Clipboard.SetContent(dataPackage);
        }
    }

    /// <summary>
    /// Handle Cancel button click event.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event args.</param>
    private async void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        await CancelDialog.ShowAsync();
    }
}

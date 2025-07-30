// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.Contracts.Views;
using WinGetStudio.ViewModels.ConfigurationFlow;
using WinGetStudio.Common.Windows.FileDialog;
using Microsoft.UI.Xaml.Controls;

namespace WinGetStudio.Views.ConfigurationFlow;

public sealed partial class SelectFilePage : Page, IView<SelectFileViewModel>
{
    public SelectFileViewModel ViewModel { get; }

    public SelectFilePage()
    {
        ViewModel = App.GetService<SelectFileViewModel>();
        this.InitializeComponent();
    }

    private async void Browse_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        try
        {
            using var fileDialog = new WindowOpenFileDialog();
            fileDialog.AddFileType("YAML files", ".yaml", ".yml", ".winget");
            var file = await fileDialog.ShowAsync(App.MainWindow);
            await ViewModel.SelectFileAsync(file);
        }
        catch
        {
            // No-op
        }
    }
}

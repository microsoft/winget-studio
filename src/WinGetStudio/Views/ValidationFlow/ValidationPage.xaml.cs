using System.Collections.ObjectModel;
using System.ComponentModel;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Contracts.Views;
using WinGetStudio.Models;
using WinGetStudio.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.ApplicationModel.DataTransfer;

namespace WinGetStudio.Views;

public sealed partial class ValidationPage : Page, IView<ValidationViewModel>
{
    public ValidationViewModel ViewModel { get; }

    public ValidationPage()
    {
        ViewModel = App.GetService<ValidationViewModel>();
        DataContext = ViewModel;
        InitializeComponent();
    }

    private void CopyResultsToClipboard()
    {
        var dataPackage = new DataPackage();
        dataPackage.SetText(ViewModel.RawData);
        Clipboard.SetContent(dataPackage);
    }
}
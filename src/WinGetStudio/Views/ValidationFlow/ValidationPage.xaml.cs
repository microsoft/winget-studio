using WinGetStudio.Contracts.Views;
using WinGetStudio.ViewModels;
using Microsoft.UI.Xaml.Controls;
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
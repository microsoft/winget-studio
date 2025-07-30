using WinGetStudio.Contracts.Views;
using WinGetStudio.ViewModels;
using WinGetStudio.Common.Windows.FileDialog;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace WinGetStudio.Views;

public sealed partial class MainPage : Page, IView<MainViewModel>
{
    public MainViewModel ViewModel { get; }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();
    }

    private async void Configuration_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var filePicker = new WindowOpenFileDialog();
            filePicker.AddFileType("Configuration Files", ".winget", ".yaml", ".yml");
            var selectedFile = await filePicker.ShowAsync(App.MainWindow);
            if (selectedFile != null)
            {
                await ViewModel.StartConfigurationFlowAsync(selectedFile);
            }
        }
        catch
        {
            // No-op
        }
    }
}

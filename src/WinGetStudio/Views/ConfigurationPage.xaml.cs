using WinGetStudio.Contracts.Views;
using WinGetStudio.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace WinGetStudio.Views;

public sealed partial class ConfigurationPage : Page, IView<ConfigurationViewModel>
{
    public ConfigurationViewModel ViewModel { get; }

    public ConfigurationPage()
    {
        ViewModel = App.GetService<ConfigurationViewModel>();
        InitializeComponent();

        ViewModel.NavigationService.Frame = NavigationFrame;
    }
}

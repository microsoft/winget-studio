using dscui.Contracts.Views;
using dscui.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace dscui.Views;

public sealed partial class ConfigurationPage : Page, IView<ConfigurationViewModel>
{
    public ConfigurationViewModel ViewModel { get; }

    public ConfigurationPage()
    {
        ViewModel = App.GetService<ConfigurationViewModel>();
        InitializeComponent();

        ViewModel.NavigationService.Frame = NavigationFrame;
        ViewModel.NavigationService.NavigateToDefaultPage();
    }
}

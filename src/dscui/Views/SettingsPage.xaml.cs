using dscui.Contracts.Views;
using dscui.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace dscui.Views;

public sealed partial class SettingsPage : Page, IView<SettingsViewModel>
{
    public SettingsViewModel ViewModel
    {
        get;
    }

    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsViewModel>();
        InitializeComponent();
    }
}

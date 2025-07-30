using WinGetStudio.Contracts.Views;
using WinGetStudio.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace WinGetStudio.Views;

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

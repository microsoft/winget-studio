using WinGetStudio.Contracts.Views;
using WinGetStudio.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace WinGetStudio.Views;
public sealed partial class ValidationFramePage : Page, IView<ValidationFrameViewModel>
{
    public ValidationFrameViewModel ViewModel { get; }

    public ValidationFramePage()
    {
        ViewModel = App.GetService<ValidationFrameViewModel>();
        InitializeComponent();

        ViewModel.NavigationService.Frame = NavigationFrame;
    }
}
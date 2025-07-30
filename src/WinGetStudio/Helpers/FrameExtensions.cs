using WinGetStudio.Contracts.Views;
using Microsoft.UI.Xaml.Controls;

namespace WinGetStudio.Helpers;

public static class FrameExtensions
{
    public static object? GetPageViewModel(this Frame frame) => frame?.Content?.GetType().GetProperty(nameof(IView<object>.ViewModel))?.GetValue(frame.Content, null);
}

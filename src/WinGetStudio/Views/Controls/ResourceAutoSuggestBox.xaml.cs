// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using WinGetStudio.ViewModels.Controls;

namespace WinGetStudio.Views.Controls;

public sealed partial class ResourceAutoSuggestBox : UserControl
{
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(ResourceAutoSuggestBox), new PropertyMetadata(null));
    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(nameof(Header), typeof(string), typeof(ResourceAutoSuggestBox), new PropertyMetadata(null));

    public ResourceAutoSuggestBoxViewModel ViewModel { get; }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public ResourceAutoSuggestBox()
    {
        ViewModel = App.GetService<ResourceAutoSuggestBoxViewModel>();
        InitializeComponent();
        BindingOperations.SetBinding(this, TextProperty, new Binding
        {
            Path = new PropertyPath(nameof(ViewModel.SearchResourceText)),
            Source = ViewModel,
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
        });
    }

    /// <summary>
    /// Opens the resource explorer dialog for the current resource.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event args.</param>
    private async void ExploreResource_Click(object sender, RoutedEventArgs e)
    {
        var resource = await ViewModel.OnExploreAsync();
        if (resource != null)
        {
            ResourceExplorerDialog.Resource = resource;
            await ResourceExplorerDialog.ShowAsync();
        }
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.WinUI;
using dscui.Contracts.Views;
using dscui.ViewModels.ConfigurationFlow;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;

namespace dscui.Views.ConfigurationFlow;

public sealed partial class PreviewFilePage : Page, IView<PreviewFileViewModel>
{
    public PreviewFileViewModel ViewModel { get; }

    public PreviewFilePage()
    {
        ViewModel = App.GetService<PreviewFileViewModel>();
        InitializeComponent();
    }

    private async void Dependency_Click(Hyperlink sender, HyperlinkClickEventArgs args)
    {
        var textBlock = sender?.ContentStart?.VisualParent as TextBlock;
        if (textBlock?.Tag is string unitId && !string.IsNullOrEmpty(unitId))
        {
            for (var i = 0; i < ConfigurationUnitSections.Items.Count; ++i)
            {
                var listViewItem = ConfigurationUnitSections.ItemContainerGenerator.ContainerFromIndex(i) as ListViewItem;
                var expander = listViewItem?.ContentTemplateRoot as Expander;
                if (expander != null && expander.Tag?.ToString() == unitId)
                {
                    expander.IsExpanded = true;
                    expander.Focus(FocusState.Programmatic);
                    await ConfigurationUnitSections.SmoothScrollIntoViewWithIndexAsync(i);
                    break;
                }
            }
        }
    }
}

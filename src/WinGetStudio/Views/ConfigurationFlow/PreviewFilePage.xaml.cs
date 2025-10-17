// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.WinUI;
using Microsoft.Extensions.Localization;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using WinGetStudio.Common.Windows.FileDialog;
using WinGetStudio.Contracts.Views;
using WingetStudio.Services.VisualFeedback.Contracts;
using WingetStudio.Services.VisualFeedback.Models;
using WinGetStudio.ViewModels;
using WinGetStudio.ViewModels.ConfigurationFlow;

namespace WinGetStudio.Views.ConfigurationFlow;

public sealed partial class PreviewFilePage : Page, IView<PreviewFileViewModel>
{
    private readonly IStringLocalizer<PreviewFilePage> _localizer;
    private readonly IUIFeedbackService _ui;

    public PreviewFileViewModel ViewModel { get; }

    public PreviewFilePage()
    {
        _localizer = App.GetService<IStringLocalizer<PreviewFilePage>>();
        _ui = App.GetService<IUIFeedbackService>();
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

    private async void OpenConfigurationFile(object sender, RoutedEventArgs e)
    {
        try
        {
            var filePicker = new WindowOpenFileDialog();
            string[] fileExts = [".winget", ".yaml", ".yml"];
            filePicker.AddFileType(_localizer["PreviewPage_FilePicker_ConfigurationFiles"], fileExts);
            var selectedFile = await filePicker.ShowAsync(App.MainWindow);
            if (selectedFile != null)
            {
                await ViewModel.OpenConfigurationFileAsync(selectedFile);
            }
        }
        catch (Exception ex)
        {
            _ui.ShowTimedNotification(ex.Message, NotificationMessageSeverity.Error);
        }
    }

    private async void SaveConfigurationFileAs(object sender, RoutedEventArgs e)
    {
        try
        {
            var filePicker = new WindowSaveFileDialog();
            filePicker.AddFileType(_localizer["PreviewPage_FilePicker_ConfigurationFiles"], ".winget");
            var selectedFile = filePicker.Show(App.MainWindow);
            if (!string.IsNullOrEmpty(selectedFile))
            {
                await ViewModel.SaveConfigurationAsAsync(selectedFile);
            }
        }
        catch (Exception ex)
        {
            _ui.ShowTimedNotification(ex.Message, NotificationMessageSeverity.Error);
        }
    }

    private void SelectedUnitDependencyChanged(object sender, SelectionChangedEventArgs e)
    {
        foreach (var id in e.AddedItems.OfType<UnitViewModel>())
        {
            ViewModel.SelectedUnit?.Item2.Dependencies?.Add(id);
        }

        foreach (var id in e.RemovedItems.OfType<UnitViewModel>())
        {
            ViewModel.SelectedUnit?.Item2.Dependencies?.Remove(id);
        }
    }

    private void SelectedUnitDependencyLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is ListView listView && ViewModel.SelectedUnit != null)
        {
            // Disable the option that matches the currently selected unit
            foreach (var item in listView.Items.OfType<UnitViewModel>())
            {
                var container = listView.ContainerFromItem(item) as ListViewItem;
                container?.IsEnabled = item != ViewModel.SelectedUnit.Item1;
            }

            // Get the set of IDs to select
            var idsToSelect = ViewModel.SelectedUnit.Item2.Dependencies?.ToHashSet();
            if (idsToSelect == null || idsToSelect.Count == 0)
            {
                listView.SelectedItems.Clear();
                return;
            }

            // Remove unneeded selections
            foreach (var selectedItem in listView.SelectedItems.OfType<UnitViewModel>())
            {
                if (!idsToSelect.Remove(selectedItem))
                {
                    listView.SelectedItems.Remove(selectedItem);
                }
            }

            // Add missing selections
            foreach (var id in idsToSelect)
            {
                listView.SelectedItems.Add(id);
            }
        }
    }
}

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
        DataContext = ViewModel;
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

    private async void ShowSaveDialog(object sender, RoutedEventArgs e)
    {
        var dialog = SaveDialog;
        await dialog.ShowAsync();
    }

    private async void ShowApplyDialog(object sender, RoutedEventArgs e)
    {
        var dialog = ApplyDialog;
        await dialog.ShowAsync();
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
}

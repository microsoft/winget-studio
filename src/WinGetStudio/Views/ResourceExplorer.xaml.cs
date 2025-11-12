// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;
using WinGetStudio.Services.Operations.Contracts;
using WinGetStudio.Services.Operations.Extensions;
using WinGetStudio.ViewModels.Controls;

namespace WinGetStudio.Views;

public sealed partial class ResourceExplorer : ContentDialog
{
    private readonly IOperationHub _operationHub;
    private readonly IStringLocalizer<ResourceExplorer> _localizer;
    private readonly ILogger<ResourceExplorer> _logger;

    public ResourceExplorerViewModel ViewModel { get; }

    public ResourceExplorer(DSCResource resource)
    {
        _operationHub = App.GetService<IOperationHub>();
        _localizer = App.GetService<IStringLocalizer<ResourceExplorer>>();
        _logger = App.GetService<ILogger<ResourceExplorer>>();
        ViewModel = App.GetService<ResourceExplorerViewModelFactory>()(resource);
        InitializeComponent();
    }

    /// <summary>
    /// Copies the property name to the clipboard when the copy button is clicked.
    /// </summary>
    /// <param name="sender">The button that was clicked.</param>
    /// <param name="e">The event data.</param>
    private void OnCopyPropertyName(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string propertyName)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(propertyName);
            Clipboard.SetContent(dataPackage);
        }
    }

    /// <summary>
    /// Closes the dialog when the close button is clicked.
    /// </summary>
    /// <param name="sender">The button that was clicked.</param>
    /// <param name="e">>The event data.</param>
    private void OnClose(object sender, RoutedEventArgs e) => Hide();

    /// <summary>
    /// Copies the sample YAML to the clipboard when the copy as YAML button is clicked.
    /// </summary>
    /// <param name="sender">The button that was clicked.</param>
    /// <param name="e">>The event data.</param>
    private async void OnCopyAsYaml(object sender, RoutedEventArgs e)
    {
        await _operationHub.ExecuteAsync(async ctx =>
        {
            try
            {
                var dataPackage = new DataPackage();
                var sampleYaml = await ViewModel.GenerateDefaultYamlAsync();
                dataPackage.SetText(sampleYaml);
                Clipboard.SetContent(dataPackage);
                ctx.Success(props => props with { Message = _localizer["ResourceExplorer_YamlCopied"] });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to copy sample YAML to clipboard.");
                ctx.Fail(props => props with { Message = _localizer["ResourceExplorer_YamlCopyFailed"] });
            }
            finally
            {
                Hide();
            }
        });
    }
}

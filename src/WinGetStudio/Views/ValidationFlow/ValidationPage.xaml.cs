// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using WinGetStudio.Contracts.Views;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;
using WingetStudio.Services.VisualFeedback.Contracts;
using WingetStudio.Services.VisualFeedback.Models;
using WinGetStudio.ViewModels;

namespace WinGetStudio.Views;

public sealed partial class ValidationPage : Page, IView<ValidationViewModel>
{
    private readonly List<string> _fullResourceNames = [];
    private IReadOnlyList<IDSCModule> _dscModules = [];

    public ValidationViewModel ViewModel { get; }

    public ValidationPage()
    {
        ViewModel = App.GetService<ValidationViewModel>();
        DataContext = ViewModel;
        InitializeComponent();
    }

    private void CopyResultsToClipboard()
    {
        var dataPackage = new DataPackage();
        dataPackage.SetText(ViewModel.RawData);
        Clipboard.SetContent(dataPackage);
    }

    private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            var query = sender.Text.Trim();
            if (string.IsNullOrEmpty(query) || _fullResourceNames.Count == 0)
            {
                sender.ItemsSource = null;
                return;
            }

            var suggestions = _fullResourceNames
                .Where(name => name.Contains(query, StringComparison.OrdinalIgnoreCase))
                .Take(10)
                .ToList();
            sender.ItemsSource = suggestions;
        }
        else
        {
            sender.ItemsSource = null;
        }
    }

    private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        // TODO
    }

    private async void Page_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var ui = App.GetService<IUIFeedbackService>();
        ui.ShowTaskProgress();
        ui.ShowOutcomeNotification(null, "Loading DSC modules...", NotificationMessageSeverity.Informational);

        var dscExplorer = App.GetService<IDSCExplorer>();
        _dscModules = await dscExplorer.GetDSCModulesAsync();
        foreach (var module in _dscModules)
        {
            await module.LoadDSCResourcesAsync();
            foreach (var resource in module.Resources)
            {
                _fullResourceNames.Add($"{module.Id}/{resource}");
            }
        }

        ui.HideTaskProgress();
        ui.ShowOutcomeNotification(null, "DSC modules loaded", NotificationMessageSeverity.Success);
    }

    private async void Button_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var fullResourceName = FullResourceName.Text.Trim();
        if (!string.IsNullOrWhiteSpace(fullResourceName))
        {
            var parts = fullResourceName.Split('/');
            if (parts.Length == 2)
            {
                var moduleName = parts[0];
                var resourceName = parts[1];
                var module = _dscModules.FirstOrDefault(m => m.Id.Equals(moduleName, StringComparison.OrdinalIgnoreCase));
                if (module != null)
                {
                    await module.LoadDSCResourcesDefinitionAsync();
                    var resource = module?.GetResourceDetails(resourceName);
                    Details.Text = string.Empty;
                    foreach (var prop in resource?.Properties.Values.ToList() ?? [])
                    {
                        var syntaxOneLiner = prop.Syntax.Replace("\n", " ").Replace("\r", " ");
                        Details.Text += $"{prop.Name} | {prop.Type} | Syntax: {syntaxOneLiner}\n";
                    }
                }
            }
        }
    }
}

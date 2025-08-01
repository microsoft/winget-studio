// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Web;
using WinGetStudio.Contracts.Services;
using WinGetStudio.ViewModels;
using Windows.ApplicationModel.Activation;

namespace WinGetStudio.Activation;
internal class ConfigurationFileActivationHandler : ActivationHandler<ProtocolActivatedEventArgs>
{
    private readonly IAppNavigationService _navigationService;
    public ConfigurationFileActivationHandler(IAppNavigationService navigationService)
    {
        _navigationService = navigationService;
    }
    public const string AppSearchUri = "ms-wingetstudio";
    protected override bool CanHandleInternal(ProtocolActivatedEventArgs args)
    {
        return args.Uri != null;
    }

    protected async override Task HandleInternalAsync(ProtocolActivatedEventArgs args)
    {
        var uri = args.Uri;

        if (uri != null)
        {
            var queryParams = HttpUtility.ParseQueryString(uri.Query);
            if (uri.Host == "file")
            {
                _navigationService.NavigateTo<ConfigurationViewModel>(queryParams["path"]);
            }
        }
        await Task.CompletedTask;
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.Contracts.Services;
using WinGetStudio.ViewModels;
using Windows.ApplicationModel.Activation;

namespace WinGetStudio.Activation;
internal class FileActivationHandler : ActivationHandler<FileActivatedEventArgs>
{
    private readonly IAppNavigationService _navigationService;
    public FileActivationHandler(IAppNavigationService navigationService)
    {
        _navigationService = navigationService;
    }
    protected override bool CanHandleInternal(FileActivatedEventArgs args)
    {
        return args.Files[0] != null;
    }

    protected async override Task HandleInternalAsync(FileActivatedEventArgs args)
    {
        var file = args.Files[0];
        _navigationService.NavigateTo<ConfigurationViewModel>(file.Path);
    }
}
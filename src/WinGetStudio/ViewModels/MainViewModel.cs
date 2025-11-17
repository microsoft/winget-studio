// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Windows.Storage;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Services;
using WinGetStudio.Services.Operations.Contracts;
using WinGetStudio.Services.Operations.Extensions;
using WinGetStudio.Services.Operations.Models;

namespace WinGetStudio.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    private readonly IAppFrameNavigationService _navigationService;
    private readonly IAppOperationHub _operationHub;

    public MainViewModel(IAppFrameNavigationService navigationService, IAppOperationHub operationHub)
    {
        _navigationService = navigationService;
        _operationHub = operationHub;
    }

    public async Task StartConfigurationFlowAsync(IStorageFile file)
    {
        _navigationService.NavigateTo<ConfigurationViewModel>(file);
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task OnNavigateToConfigurationAsync()
    {
        _navigationService.NavigateTo<ConfigurationViewModel>();
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task OnNavigateToValidationAsync()
    {
        _navigationService.NavigateTo<ValidationViewModel>();
        await Task.CompletedTask;
    }

    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task Button1Async()
    {
        await _operationHub.ExecuteAsync(
            AppOperationHub.PassiveOptions,
            async ctx =>
            {
                ctx.StartSnapshotBroadcast();
                ctx.Start();
                ctx.AddCancelAction("Cancel 1", false);
                ctx.AddCancelAction("Cancel 2");
                var progress = 0;
                while (progress < 100)
                {
                    await Task.Delay(500);
                    progress += 10;
                    ctx.ReportProgress(progress);
                }

                ctx.Success(props => props with { Title = Guid.NewGuid().ToString(), Message = Guid.NewGuid().ToString() });
                ctx.AddDoneAction("Done 1", false);
                ctx.AddDoneAction("Done 2");
            });
    }
}

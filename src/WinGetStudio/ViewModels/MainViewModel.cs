// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Windows.Storage;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Services.Operations.Contracts;
using WinGetStudio.Services.Operations.Extensions;
using WinGetStudio.Services.Operations.Models.Policies;
using WinGetStudio.Services.Operations.Models.States;

namespace WinGetStudio.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    private readonly IAppFrameNavigationService _navigationService;
    private readonly IOperationHub _operationHub;

    public MainViewModel(IAppFrameNavigationService navigationService, IOperationHub operationHub)
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
        var options = new OperationExecutionOptions([
            new SnapshotRetentionPolicy(OperationStatus.Completed, OperationSeverity.Success, TimeSpan.FromSeconds(3)),
        ]);
        await _operationHub.ExecuteAsync(
            async ctx =>
            {
                ctx.StartSnapshotBroadcast();
                ctx.Start();
                var progress = 0;
                while (progress < 100)
                {
                    await Task.Delay(500);
                    progress += 10;
                    ctx.ReportProgress(progress);
                }

                ctx.Success(props => props with { Title = Guid.NewGuid().ToString(), Message = Guid.NewGuid().ToString() });
            },
            options);
    }
}

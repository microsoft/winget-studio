// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using WinGetStudio.Contracts.Services;

namespace WinGetStudio.ViewModels.ConfigurationFlow;

public partial class ApplyFileViewModel : ObservableRecipient
{
    private readonly IConfigurationFrameNavigationService _navigationService;
    private readonly IAppOperationHub _operationHub;
    private readonly ILogger<ApplyFileViewModel> _logger;
    private readonly IStringLocalizer<ApplyFileViewModel> _localizer;
    private readonly IConfigurationManager _manager;
    private readonly ApplySetViewModelFactory _applySetFactory;

    [ObservableProperty]
    public partial ApplySetViewModel? ApplySet { get; set; }

    public ApplyFileViewModel(
        IConfigurationFrameNavigationService navigationService,
        IAppOperationHub operationHub,
        ILogger<ApplyFileViewModel> logger,
        IStringLocalizer<ApplyFileViewModel> localizer,
        IConfigurationManager manager,
        ApplySetViewModelFactory applySetFactory)
    {
        _navigationService = navigationService;
        _logger = logger;
        _localizer = localizer;
        _operationHub = operationHub;
        _manager = manager;
        _applySetFactory = applySetFactory;
    }

    [RelayCommand]
    private async Task OnLoadedAsync()
    {
        if (_manager.ActiveSetApplyState.CanRestoreState())
        {
            _logger.LogInformation("Restoring previous apply configuration set state");
            _manager.ActiveSetApplyState.RestoreState(this);
        }
        else
        {
            _logger.LogInformation("Starting to apply configuration set");
            await ApplyPreviewSetAsync();
        }
    }

    /// <summary>
    /// Applies the currently active preview configuration set.
    /// </summary>
    private async Task ApplyPreviewSetAsync()
    {
        var activeSet = _manager.ActiveSetPreviewState.ActiveSet;
        Debug.Assert(activeSet != null, "ActiveSet should not be null when applying configuration set.");

        await _operationHub.RunWithProgressAsync(
            props => props with { Title = _localizer["ApplySetOperation_Title"], Message = _localizer["ApplySetOperation_PreStartMessage"] },
            async (context, factory) =>
            {
                var dscFile = activeSet.GetLatestDSCFile();
                var openSet = factory.CreateOpenSetOperation(dscFile);
                var openSetResult = await openSet.ExecuteAsync(context);
                if (openSetResult.IsSuccess && openSetResult.Result != null)
                {
                    ApplySet = _applySetFactory(openSetResult.Result);
                    _manager.ActiveSetApplyState.CaptureState(this);
                    await ApplySet.ApplyAsync(context);
                }
            });
    }

    [RelayCommand]
    private void OnDone()
    {
        _manager.ActiveSetApplyState.ClearState();
        _navigationService.NavigateToDefaultPage();
    }

    [RelayCommand]
    private void OnBack()
    {
        _navigationService.NavigateToDefaultPage();
    }
}

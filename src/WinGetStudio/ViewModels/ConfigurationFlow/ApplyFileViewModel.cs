// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Exceptions;
using WingetStudio.Services.VisualFeedback.Contracts;
using WingetStudio.Services.VisualFeedback.Models;

namespace WinGetStudio.ViewModels.ConfigurationFlow;

public partial class ApplyFileViewModel : ObservableRecipient
{
    private readonly IStringLocalizer<ApplyFileViewModel> _localizer;
    private readonly IConfigurationFrameNavigationService _navigationService;
    private readonly IDSC _dsc;
    private readonly IUIFeedbackService _ui;
    private readonly ILogger _logger;
    private readonly IConfigurationManager _manager;
    private readonly ApplySetViewModelFactory _applySetFactory;

    [ObservableProperty]
    public partial ApplySetViewModel? ApplySet { get; set; }

    public ApplyFileViewModel(
        IConfigurationFrameNavigationService navigationService,
        IDSC dsc,
        IUIFeedbackService ui,
        IStringLocalizer<ApplyFileViewModel> localizer,
        ILogger<ApplyFileViewModel> logger,
        IConfigurationManager manager,
        ApplySetViewModelFactory applySetFactory)
    {
        _navigationService = navigationService;
        _dsc = dsc;
        _logger = logger;
        _localizer = localizer;
        _ui = ui;
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
        try
        {
            _ui.ShowTaskProgress();
            _logger.LogInformation($"Applying configuration set started");
            var dscFile = activeSet.GetLatestDSCFile();
            var dscSet = await _dsc.OpenConfigurationSetAsync(dscFile);
            ApplySet = _applySetFactory(dscSet);
            _manager.ActiveSetApplyState.CaptureState(this);
            await ApplySet.ApplyAsync();
        }
        catch (OpenConfigurationSetException ex)
        {
            _logger.LogError(ex, $"Opening configuration set failed during apply");
            _ui.ShowTimedNotification(ex.GetErrorMessage(_localizer), NotificationMessageSeverity.Error);
        }
        catch (ApplyConfigurationSetException ex)
        {
            _logger.LogError(ex, $"Applying configuration set failed");
            _ui.ShowTimedNotification(ex.GetSetErrorMessage(_localizer), NotificationMessageSeverity.Error);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Applying configuration set was canceled by user");
            _ui.ShowTimedNotification(_localizer["ApplyFile_ApplyOperationCanceledMessage"], NotificationMessageSeverity.Warning);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unknown error while validating configuration code");
            _ui.ShowTimedNotification(ex.Message, NotificationMessageSeverity.Error);
        }
        finally
        {
            _ui.HideTaskProgress();
        }
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

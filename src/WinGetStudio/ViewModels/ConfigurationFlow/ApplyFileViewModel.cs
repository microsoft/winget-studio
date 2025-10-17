// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Management.Configuration;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Models;
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
    private readonly IUIDispatcher _dispatcher;
    private readonly IConfigurationManager _manager;

    [ObservableProperty]
    public partial ApplySetViewModel? ApplySet { get; set; }

    public ApplyFileViewModel(
        IConfigurationFrameNavigationService navigationService,
        IDSC dsc,
        IUIDispatcher dispatcher,
        IUIFeedbackService ui,
        IStringLocalizer<ApplyFileViewModel> localizer,
        ILogger<ApplyFileViewModel> logger,
        IConfigurationManager manager)
    {
        _navigationService = navigationService;
        _dsc = dsc;
        _logger = logger;
        _dispatcher = dispatcher;
        _localizer = localizer;
        _ui = ui;
        _manager = manager;
    }

    [RelayCommand]
    private async Task OnLoadedAsync()
    {
        var applyState = _manager.ActiveSetApplyState;
        if (applyState.ActiveApplySet == null)
        {
            _logger.LogInformation("Starting to apply configuration set");
            await ApplyPreviewSetAsync();
        }
        else
        {
            _logger.LogInformation("Resuming application of configuration set");
            ApplySet = applyState.ActiveApplySet;
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
            ApplySet = new ApplySetViewModel(_localizer, dscSet);
            _manager.ActiveSetApplyState.ActiveApplySet = ApplySet;
            var applySetTask = _dsc.ApplySetAsync(dscSet);
            applySetTask.Progress += OnDataChanged;
            await applySetTask;
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

    private async void OnDataChanged(object? sender, IDSCSetChangeData data)
    {
        await _dispatcher.EnqueueAsync(() => OnDataChangedInternal(data));
    }

    private void OnDataChangedInternal(IDSCSetChangeData data)
    {
        if (data.Change == ConfigurationSetChangeEventType.UnitStateChanged && data.Unit != null && ApplySet != null)
        {
            var unit = ApplySet.Units.FirstOrDefault(u => u.Unit.InstanceId == data.Unit.InstanceId);
            if (unit != null)
            {
                if (data.UnitState == ConfigurationUnitState.InProgress)
                {
                    unit.Update(ApplyUnitState.InProgress);
                }
                else if (data.UnitState == ConfigurationUnitState.Skipped)
                {
                    unit.Update(ApplyUnitState.Skipped, data.ResultInformation);
                }
                else if (data.UnitState == ConfigurationUnitState.Completed)
                {
                    var state = data.ResultInformation.IsOk ? ApplyUnitState.Succeeded : ApplyUnitState.Failed;
                    unit.Update(state, data.ResultInformation);
                }
            }
        }
    }

    [RelayCommand]
    private async Task OnDoneAsync()
    {
        _manager.ActiveSetApplyState.ActiveApplySet = null;
        _navigationService.NavigateToDefaultPage();
        await Task.CompletedTask;
    }
}

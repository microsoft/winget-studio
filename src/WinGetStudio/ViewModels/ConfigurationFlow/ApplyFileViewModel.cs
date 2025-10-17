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
    private readonly IUIDispatcher _dispatcher;
    private readonly IConfigurationManager _manager;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsReadyToCompleted))]
    [NotifyCanExecuteChangedFor(nameof(DoneCommand))]
    public partial ApplySetViewModel? ApplySet { get; set; }

    public bool IsReadyToCompleted => ApplySet?.IsCompleted ?? false;

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
        if (CanRestoreState())
        {
            _logger.LogInformation("Restoring previous apply configuration set state");
            RestoreState();
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
            ApplySet = new ApplySetViewModel(_localizer, dscSet);
            CaptureState();
            var applySetTask = _dsc.ApplySetAsync(dscSet);
            applySetTask.Progress += (_, data) => OnDataChanged(ApplySet, data);
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

    private async void OnDataChanged(ApplySetViewModel applySet, IDSCSetChangeData data)
    {
        await _dispatcher.EnqueueAsync(() => applySet.OnDataChanged(data));
    }

    [RelayCommand(CanExecute = nameof(IsReadyToCompleted))]
    private void OnDone()
    {
        ClearState();
        _navigationService.NavigateToDefaultPage();
    }

    [RelayCommand]
    private void OnBack()
    {
        _navigationService.NavigateToDefaultPage();
    }

    private bool CanRestoreState() => _manager.ActiveSetApplyState.ActiveApplySet != null;

    private void CaptureState()
    {
        _manager.ActiveSetApplyState.ActiveApplySet = ApplySet;
    }

    private void RestoreState()
    {
        ApplySet = _manager.ActiveSetApplyState.ActiveApplySet;
    }

    private void ClearState()
    {
        _manager.ActiveSetApplyState.ActiveApplySet = null;
    }

    partial void OnApplySetChanged(ApplySetViewModel? oldValue, ApplySetViewModel? newValue)
    {
        oldValue?.Completed -= OnApplySetCompleted;
        newValue?.Completed += OnApplySetCompleted;
    }

    private void OnApplySetCompleted(object? sender, EventArgs e)
    {
        // Notify is ready to complete
        OnPropertyChanged(nameof(IsReadyToCompleted));
        DoneCommand.NotifyCanExecuteChanged();
    }
}

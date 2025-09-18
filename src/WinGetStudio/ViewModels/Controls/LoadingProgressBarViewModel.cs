// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using Microsoft.UI.Dispatching;
using WingetStudio.Services.VisualFeedback.Contracts;

namespace WinGetStudio.ViewModels.Controls;

public partial class LoadingProgressBarViewModel : ObservableRecipient
{
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly IUIFeedbackService _uiFeedbackService;

    [ObservableProperty]
    public partial int ProgressValue { get; set; }

    [ObservableProperty]
    public partial bool IsProgressIndeterminate { get; set; }

    [ObservableProperty]
    public partial bool IsProgressVisible { get; set; }

    public LoadingProgressBarViewModel(IUIFeedbackService uiFeedbackService)
    {
        _uiFeedbackService = uiFeedbackService;
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
    }

    [RelayCommand]
    private void OnLoaded()
    {
        _uiFeedbackService.Loading.StateChanged += OnLoadingStateChanged;
    }

    [RelayCommand]
    private void OnUnloaded()
    {
        _uiFeedbackService.Loading.StateChanged -= OnLoadingStateChanged;
    }

    private async void OnLoadingStateChanged(object? sender, EventArgs e)
    {
        await _dispatcherQueue.EnqueueAsync(() =>
        {
            ProgressValue = _uiFeedbackService.Loading.ProgressValue;
            IsProgressIndeterminate = _uiFeedbackService.Loading.IsIndeterminate;
            IsProgressVisible = _uiFeedbackService.Loading.IsVisible;
        });
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Navigation;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Services.Operations.Contracts;
using WinGetStudio.Services.Operations.Models.States;
using WinGetStudio.Views;

namespace WinGetStudio.ViewModels;

public partial class ShellViewModel : ObservableRecipient, IDisposable
{
    private readonly IOperationHub _ops;
    private readonly IDisposable _activitySubscription;
    private bool _disposedValue;

    [ObservableProperty]
    public partial bool IsBackEnabled { get; set; }

    [ObservableProperty]
    public partial object? Selected { get; set; }

    [ObservableProperty]
    public partial bool IsNotificationPaneOpen { get; set; }

    [ObservableProperty]
    public partial int UnreadNotificationsCount { get; set; }

    public IAppFrameNavigationService NavigationService { get; }

    public IAppShellNavigationViewService NavigationViewService { get; }

    public ShellViewModel(
        IAppFrameNavigationService navigationService,
        IAppShellNavigationViewService navigationViewService,
        IOperationHub ops)
    {
        NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;
        NavigationViewService = navigationViewService;
        _ops = ops;
        _activitySubscription = _ops.GlobalActivity.Subscribe(OnGlobalActivity);
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        IsBackEnabled = NavigationService.CanGoBack;

        if (e.SourcePageType == typeof(SettingsPage))
        {
            Selected = NavigationViewService.SettingsItem;
            return;
        }

        var selectedItem = NavigationViewService.GetSelectedItem(e.SourcePageType);
        if (selectedItem != null)
        {
            Selected = selectedItem;
        }
    }

    [RelayCommand]
    private void OnLoaded()
    {
    }

    [RelayCommand]
    private void OnUnloaded()
    {
    }

    [RelayCommand]
    private void OnToggleNotificationPane()
    {
        IsNotificationPaneOpen = !IsNotificationPaneOpen;
    }

    private void OnGlobalActivity(GlobalActivity activity)
    {
        UnreadNotificationsCount = activity.InProgressCount;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _activitySubscription.Dispose();
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Contracts.ViewModels;
using WingetStudio.Services.VisualFeedback.Contracts;

namespace WinGetStudio.ViewModels;

public partial class ValidationFrameViewModel : ObservableRecipient, INavigationAware
{
    private readonly IUIFeedbackService _ui;

    public IValidationNavigationService NavigationService { get; }

    public ValidationFrameViewModel(IValidationNavigationService navigationService, IUIFeedbackService ui)
    {
        NavigationService = navigationService;
        _ui = ui;
    }

    public void OnNavigatedTo(object parameter)
    {
        NavigationService.NavigateTo<ValidationViewModel>(parameter);
    }

    public void OnNavigatedFrom()
    {
        // TODO: Remove this class since we don't need a frame for this page
        // https://github.com/microsoft/winget-studio/issues/88
        _ui.ClearOverlayNotifications();
    }
}

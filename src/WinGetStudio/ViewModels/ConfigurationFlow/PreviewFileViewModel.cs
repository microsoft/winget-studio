// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Models;

namespace WinGetStudio.ViewModels.ConfigurationFlow;

public partial class PreviewFileViewModel : ObservableRecipient
{
    private readonly IConfigurationManager _manager;
    private readonly PreviewSetViewModelFactory _setFactory;
    private readonly IConfigurationFrameNavigationService _navigationService;

    public IReadOnlyList<UnitSecurityContext> SecurityContexts => UnitSecurityContext.All;

    [ObservableProperty]
    public partial PreviewSetViewModel? PreviewSet { get; set; }

    public PreviewFileViewModel(IConfigurationManager manager, PreviewSetViewModelFactory setFactory, IConfigurationFrameNavigationService navigationService)
    {
        _manager = manager;
        _setFactory = setFactory;
        _navigationService = navigationService;
    }

    [RelayCommand]
    private void OnLoaded()
    {
        if (_manager.ActiveSetPreviewState.CanRestoreState())
        {
            _manager.ActiveSetPreviewState.RestoreState(this);
        }
        else
        {
            PreviewSet ??= _setFactory();
            _manager.ActiveSetPreviewState.CaptureState(this);
        }
    }

    [RelayCommand]
    private void OnUnloaded()
    {
        _manager.ActiveSetPreviewState.CaptureState(this);
    }

    /// <summary>
    /// Saves the current preview state and navigates to the DAG visualization page.
    /// </summary>
    [RelayCommand]
    private void NavigateToDag()
    {
        _manager.ActiveSetPreviewState.CaptureState(this);
        _navigationService.NavigateTo<DagViewModel>();
    }
}

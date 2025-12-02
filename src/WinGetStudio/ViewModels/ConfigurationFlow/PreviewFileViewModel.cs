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

    public IReadOnlyList<UnitSecurityContext> SecurityContexts => UnitSecurityContext.All;

    [ObservableProperty]
    public partial PreviewSetViewModel? PreviewSet { get; set; }

    public PreviewFileViewModel(IConfigurationManager manager, PreviewSetViewModelFactory setFactory)
    {
        _manager = manager;
        _setFactory = setFactory;

        // Restore previous state if available
        RestoreState();
        PreviewSet ??= _setFactory();
    }

    [RelayCommand]
    private void OnLoaded()
    {
    }

    [RelayCommand]
    private void OnUnloaded()
    {
        _manager.ActiveSetPreviewState.CaptureState(this);
    }

    private void RestoreState()
    {
        if (_manager.ActiveSetPreviewState.CanRestoreState())
        {
            _manager.ActiveSetPreviewState.RestoreState(this);
        }
    }
}

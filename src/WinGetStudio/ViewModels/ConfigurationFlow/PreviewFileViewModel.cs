// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WinGetStudio.ViewModels.ConfigurationFlow;

public partial class PreviewFileViewModel : ObservableRecipient
{
    [ObservableProperty]
    public partial bool IsEditMode { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<DSCUnitViewModel> ConfigurationUnits { get; set; } = [];

    [ObservableProperty]
    public partial DSCUnitViewModel SelectedUnit { get; set; }

    [RelayCommand]
    private async Task OnSaveAsync()
    {
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task OnSaveAsAsync()
    {
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task OnApplyAsync()
    {
        await Task.CompletedTask;
    }
}

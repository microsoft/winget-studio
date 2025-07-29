// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using dscui.Contracts.Services;
using dscui.Contracts.ViewModels;
using DSCUI.Services.DesiredStateConfiguration.Contracts;

namespace dscui.ViewModels.ConfigurationFlow;

public partial class PreviewFileViewModel : ObservableRecipient, INavigationAware
{
    private readonly IConfigurationNavigationService _navigationService;
    private readonly IDSC _dsc;
    private IDSCSet? _dscSet;

    public ObservableCollection<DSCConfigurationUnitViewModel> ConfigurationUnits { get; } = [];

    public PreviewFileViewModel(IConfigurationNavigationService navigationService, IDSC dsc)
    {
        _navigationService = navigationService;
        _dsc = dsc;
    }

    public void OnNavigatedTo(object parameter)
    {
        if (parameter is IDSCSet dscSet)
        {
            _dscSet = dscSet;
            _dsc.GetConfigurationUnitDetails(dscSet);
            foreach (var item in dscSet.Units)
            {
                ConfigurationUnits.Add(new (item));
            }
        }
    }

    public void OnNavigatedFrom()
    {
        // No-op
    }

    [RelayCommand]
    private async Task OnBackAsync()
    {
        _navigationService.NavigateToDefaultPage();
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task OnApplyAsync()
    {
        if( _dscSet != null)
        {
            _navigationService.NavigateTo<ApplyFileViewModel>(_dscSet);
            await Task.CompletedTask;
        }
    }
}

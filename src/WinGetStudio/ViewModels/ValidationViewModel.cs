// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Contracts.ViewModels;
using WinGetStudio.Models;

namespace WinGetStudio.ViewModels;

public partial class ValidationViewModel : ObservableRecipient, INavigationAware
{
    private readonly IConfigurationManager _manager;
    private readonly ValidateUnitViewModelFactory _validateUnitFactory;

    public ObservableCollection<ValidateUnitViewModel> ValidateUnitList { get; } = [];

    [ObservableProperty]
    public partial ValidateUnitViewModel? SelectedUnit { get; set; }

    public ValidationViewModel(IConfigurationManager manager, ValidateUnitViewModelFactory validateUnitFactory)
    {
        _manager = manager;
        _validateUnitFactory = validateUnitFactory;
    }

    public void OnNavigatedTo(object parameter)
    {
        if (parameter is ValidateUnitNavigationContext context)
        {
            var validateUnit = _validateUnitFactory();
            validateUnit.SearchResourceText = context.UnitToValidate.Title;
            validateUnit.SettingsText = context.UnitToValidate.SettingsText;
            ValidateUnitList.Add(validateUnit);
        }
        else if (_manager.ActiveValidateUnitState.CanRestoreState())
        {
            _manager.ActiveValidateUnitState.RestoreState(this);
        }
        else if (ValidateUnitList.Count == 0)
        {
            ValidateUnitList.Add(_validateUnitFactory());
        }
    }

    public void OnNavigatedFrom()
    {
        _manager.ActiveValidateUnitState.CaptureState(this);
    }
}

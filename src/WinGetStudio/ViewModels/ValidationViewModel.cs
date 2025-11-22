// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Contracts.ViewModels;
using WinGetStudio.Models;

namespace WinGetStudio.ViewModels;

public partial class ValidationViewModel : ObservableRecipient, INavigationAware
{
    private readonly IConfigurationManager _manager;
    private readonly ValidateUnitViewModelFactory _validateUnitFactory;

    [ObservableProperty]
    public partial ValidateUnitViewModel? ValidateUnit { get; set; }

    public ValidationViewModel(IConfigurationManager manager, ValidateUnitViewModelFactory validateUnitFactory)
    {
        _manager = manager;
        _validateUnitFactory = validateUnitFactory;
    }

    public void OnNavigatedTo(object parameter)
    {
        if (parameter is ValidateUnitNavigationContext context)
        {
            ValidateUnit = _validateUnitFactory();
            ValidateUnit.SearchResourceText = context.UnitToValidate.Title;
            ValidateUnit.SettingsText = context.UnitToValidate.SettingsText;
        }
        else if (_manager.ActiveValidateUnitState.CanRestoreState())
        {
            _manager.ActiveValidateUnitState.RestoreState(this);
        }
        else
        {
            ValidateUnit = _validateUnitFactory();
        }
    }

    public void OnNavigatedFrom()
    {
        _manager.ActiveValidateUnitState.CaptureState(this);
    }
}

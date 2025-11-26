// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Contracts.ViewModels;
using WinGetStudio.Models;

namespace WinGetStudio.ViewModels;

public partial class ValidationViewModel : ObservableRecipient, INavigationAware
{
    private readonly IConfigurationManager _manager;
    private readonly ValidateUnitViewModelFactory _validateUnitFactory;
    private readonly ILogger<ValidationViewModel> _logger;

    public ObservableCollection<ValidateUnitViewModel> ValidateUnitList { get; } = [];

    [ObservableProperty]
    public partial ValidateUnitViewModel? SelectedUnit { get; set; }

    public ValidationViewModel(
        ILogger<ValidationViewModel> logger,
        IConfigurationManager manager,
        ValidateUnitViewModelFactory validateUnitFactory)
    {
        _logger = logger;
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
            SelectedUnit = validateUnit;
        }
        else if (_manager.ActiveValidateUnitState.CanRestoreState())
        {
            _manager.ActiveValidateUnitState.RestoreState(this);
        }
        else if (ValidateUnitList.Count == 0)
        {
            var validateUnit = _validateUnitFactory();
            ValidateUnitList.Add(validateUnit);
            SelectedUnit = validateUnit;
        }
    }

    public void OnNavigatedFrom()
    {
        _manager.ActiveValidateUnitState.CaptureState(this);
    }

    [RelayCommand]
    private void OnAddUnitValidation()
    {
        _logger.LogInformation("Adding new unit validation tab.");
        AddUnitValidation();
    }

    public void RemoveUnitValidation(ValidateUnitViewModel unit)
    {
        _logger.LogInformation("Removing unit validation tab.");
        ValidateUnitList.Remove(unit);
        if (ValidateUnitList.Count == 0)
        {
            _logger.LogInformation("No more unit validation tabs. Adding a new one.");
            AddUnitValidation();
        }
    }

    private void AddUnitValidation()
    {
        var validateUnit = _validateUnitFactory();
        ValidateUnitList.Add(validateUnit);
        SelectedUnit = validateUnit;
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Localization;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Exceptions;
using WinGetStudio.ViewModels;

namespace WinGetStudio.Models;

public partial class ApplySetUnitViewModel : ObservableObject
{
    private readonly IStringLocalizer _localizer;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLoading))]
    [NotifyPropertyChangedFor(nameof(IsExpanded))]
    public partial ApplySetUnitState State { get; set; }

    [ObservableProperty]
    public partial string? Message { get; set; }

    [ObservableProperty]
    public partial string? Description { get; set; }

    public bool IsLoading => State == ApplySetUnitState.InProgress;

    public bool IsExpanded => State == ApplySetUnitState.Failed || State == ApplySetUnitState.Skipped;

    public DSCUnitViewModel Unit { get; }

    public ApplySetUnitViewModel(IDSCUnit unit, IStringLocalizer localizer)
    {
        Unit = new(unit);
        _localizer = localizer;
        Update(ApplySetUnitState.NotStarted);
    }

    public void Update(ApplySetUnitState state, IDSCUnitResultInformation? resultInformation = null)
    {
        State = state;
        if (State == ApplySetUnitState.Succeeded)
        {
            Message = _localizer["ConfigurationUnitSuccess"];
        }
        else if (State == ApplySetUnitState.NotStarted)
        {
            Message = _localizer["ConfigurationUnitNotStarted"];
        }
        else if (State == ApplySetUnitState.Failed && resultInformation != null)
        {
            Message = ApplyConfigurationSetException.GetUnitErrorMessage(_localizer, Unit.Unit, resultInformation);
            Description = ApplyConfigurationSetException.GetErrorDescription(resultInformation);
        }
        else if (State == ApplySetUnitState.Skipped && resultInformation != null)
        {
            Message = ApplyConfigurationSetException.GetUnitSkipMessage(_localizer, resultInformation);
            Description = ApplyConfigurationSetException.GetErrorDescription(resultInformation);
        }
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Localization;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Exceptions;

namespace WinGetStudio.Models;

public partial class ApplyUnitViewModel : ObservableObject
{
    private readonly IStringLocalizer _localizer;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLoading))]
    [NotifyPropertyChangedFor(nameof(IsExpanded))]
    [NotifyPropertyChangedFor(nameof(IsCompleted))]
    public partial ApplyUnitState State { get; set; }

    [ObservableProperty]
    public partial string? Message { get; set; }

    [ObservableProperty]
    public partial string? Description { get; set; }

    public bool IsLoading => State == ApplyUnitState.InProgress;

    public bool IsExpanded => State == ApplyUnitState.Failed || State == ApplyUnitState.Skipped;

    public bool IsCompleted => State == ApplyUnitState.Succeeded || State == ApplyUnitState.Failed || State == ApplyUnitState.Skipped;

    public IDSCUnit Unit { get; }

    public string Title => Unit.ModuleName == string.Empty ? Unit.Type : $"{Unit.ModuleName}/{Unit.Type}";

    public ApplyUnitViewModel(IStringLocalizer localizer, IDSCUnit unit)
    {
        Unit = unit;
        _localizer = localizer;
        Update(ApplyUnitState.NotStarted);
    }

    public void Update(ApplyUnitState state, IDSCUnitResultInformation? resultInformation = null)
    {
        State = state;
        if (State == ApplyUnitState.Succeeded)
        {
            Message = _localizer["ConfigurationUnitSuccess"];
        }
        else if (State == ApplyUnitState.NotStarted)
        {
            Message = _localizer["ConfigurationUnitNotStarted"];
        }
        else if (State == ApplyUnitState.Failed && resultInformation != null)
        {
            Message = ApplyConfigurationSetException.GetUnitErrorMessage(_localizer, Unit, resultInformation);
            Description = ApplyConfigurationSetException.GetErrorDescription(resultInformation);
        }
        else if (State == ApplyUnitState.Skipped && resultInformation != null)
        {
            Message = ApplyConfigurationSetException.GetUnitSkipMessage(_localizer, resultInformation);
            Description = ApplyConfigurationSetException.GetErrorDescription(resultInformation);
        }
    }
}

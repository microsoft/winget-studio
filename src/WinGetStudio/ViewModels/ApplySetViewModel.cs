// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Localization;
using Microsoft.Management.Configuration;
using WinGetStudio.Models;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;

namespace WinGetStudio.ViewModels;

public sealed partial class ApplySetViewModel : ObservableObject
{
    private readonly ObservableCollection<ApplyUnitViewModel> _units;
    private readonly IStringLocalizer _localizer;

    [ObservableProperty]
    public partial bool IsCompleted { get; set; }

    public int TotalUnits => Units.Count;

    public int TotalCompletedUnits => Units.Count(u => u.IsCompleted);

    public string Summary => _localizer["ApplySet_TotalUnitsCompleted", TotalUnits == 0 ? 0 : (int)((double)TotalCompletedUnits / TotalUnits * 100)];

    /// <summary>
    /// Event raised when the apply set is completed.
    /// </summary>
    public event EventHandler? Completed;

    public ReadOnlyObservableCollection<ApplyUnitViewModel> Units { get; }

    public ApplySetViewModel(IStringLocalizer localizer, IDSCSet applySet)
    {
        _localizer = localizer;
        _units = [.. applySet.Units.Select(unit => new ApplyUnitViewModel(localizer, unit))];
        Units = new(_units);
    }

    /// <summary>
    /// Handles data change events from the DSC service.
    /// </summary>
    /// <param name="data">The change data.</param>
    public void OnDataChanged(IDSCSetChangeData data)
    {
        if (data.Change == ConfigurationSetChangeEventType.SetStateChanged && data.SetState == ConfigurationSetState.Completed)
        {
            IsCompleted = true;
        }
        else if (data.Change == ConfigurationSetChangeEventType.UnitStateChanged && data.Unit != null)
        {
            var unit = Units.FirstOrDefault(u => u.Unit.InstanceId == data.Unit.InstanceId);
            if (unit != null)
            {
                if (data.UnitState == ConfigurationUnitState.InProgress)
                {
                    unit.Update(ApplyUnitState.InProgress);
                }
                else if (data.UnitState == ConfigurationUnitState.Skipped)
                {
                    unit.Update(ApplyUnitState.Skipped, data.ResultInformation);
                }
                else if (data.UnitState == ConfigurationUnitState.Completed)
                {
                    var state = data.ResultInformation.IsOk ? ApplyUnitState.Succeeded : ApplyUnitState.Failed;
                    unit.Update(state, data.ResultInformation);
                }

                // Notify summary properties
                OnPropertyChanged(nameof(TotalCompletedUnits));
                OnPropertyChanged(nameof(Summary));
            }
        }
    }

    partial void OnIsCompletedChanged(bool value)
    {
        if (value)
        {
            Completed?.Invoke(this, EventArgs.Empty);
        }
    }
}

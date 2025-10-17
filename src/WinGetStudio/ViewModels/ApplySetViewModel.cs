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
    [ObservableProperty]
    public partial bool IsCompleted { get; set; }

    /// <summary>
    /// Event raised when the apply set is completed.
    /// </summary>
    public event EventHandler? Completed;

    public ObservableCollection<ApplyUnitViewModel> Units { get; }

    public ApplySetViewModel(IStringLocalizer localizer, IDSCSet applySet)
    {
        Units = [..applySet.Units.Select(unit => new ApplyUnitViewModel(localizer, unit))];
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

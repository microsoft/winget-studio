// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinGetStudio.Services.Operations.Contracts;
using WinGetStudio.Services.Operations.Models.States;

namespace WinGetStudio.ViewModels.Controls;

public sealed partial class ActivityViewModel : ObservableObject
{
    private readonly IOperationHub _operationHub;

    public Guid Id { get; }

    [ObservableProperty]
    public partial string? Title { get; set; }

    [ObservableProperty]
    public partial string? Message { get; set; }

    [ObservableProperty]
    public partial OperationSeverity Severity { get; set; } = OperationSeverity.Info;

    [ObservableProperty]
    public partial bool CanDismiss { get; set; } = false;

    [ObservableProperty]
    public partial int ProgressValue { get; set; }

    [ObservableProperty]
    public partial bool IsProgressIndeterminate { get; set; }

    public ObservableCollection<ActivityActionViewModel> Actions { get; }

    public ActivityViewModel(IOperationHub operationHub, OperationSnapshot snapshot)
    {
        _operationHub = operationHub;
        Id = snapshot.Id;
        Actions = [];
        Update(snapshot);
    }

    /// <summary>
    /// Update the activity view model with the latest snapshot data.
    /// </summary>
    /// <param name="snapshot">The operation snapshot.</param>
    public void Update(OperationSnapshot snapshot)
    {
        var props = snapshot.Properties;
        Title = props.Title;
        Message = props.Message;
        Severity = props.Severity;
        CanDismiss = IsDismissable(props);
        ProgressValue = props.Percent ?? 0;
        IsProgressIndeterminate = props.Percent == null;
        UpdateActions(props.Actions);
    }

    /// <summary>
    /// Update the actions associated with the activity.
    /// </summary>
    /// <param name="actions">The list of operation actions.</param>
    private void UpdateActions(IReadOnlyList<OperationAction> actions)
    {
        actions ??= [];

        // Remove actions that are no longer present
        var actionIds = actions.Select(s => s.Id).ToHashSet();
        var actionsToRemove = Actions.Where(a => !actionIds.Contains(a.Id)).ToList();
        foreach (var action in actionsToRemove)
        {
            Actions.Remove(action);
        }

        // Reorder to match the actions order and update existing actions
        for (var i = 0; i < actions.Count; i++)
        {
            var action = actions[i];
            var existingAction = Actions.FirstOrDefault(a => a.Id == action.Id);
            if (existingAction != null)
            {
                existingAction.Update(action);
                var currentIndex = Actions.IndexOf(existingAction);
                if (currentIndex != i)
                {
                    Actions.Move(currentIndex, i);
                }
            }
            else
            {
                Actions.Insert(i, new(action));
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether the activity can be dismissed.
    /// </summary>
    /// <param name="props">The operation properties.</param>
    /// <returns>True if the activity can be dismissed; otherwise, false.</returns>
    private bool IsDismissable(OperationProperties props)
    {
        return props.Status == OperationStatus.Completed || props.Status == OperationStatus.Canceled;
    }

    [RelayCommand]
    private void OnDismiss()
    {
        _operationHub.StopSnapshotBroadcast(Id);
    }
}

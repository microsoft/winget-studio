// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using WinGetStudio.Services.Operations.Models.State;

namespace WinGetStudio.ViewModels.Controls;

public sealed partial class ActivityViewModel : ObservableObject
{
    public Guid Id { get; }

    [ObservableProperty]
    public partial string? Title { get; set; }

    [ObservableProperty]
    public partial string? Message { get; set; }

    [ObservableProperty]
    public partial OperationSeverity Severity { get; set; } = OperationSeverity.Info;

    [ObservableProperty]
    public partial bool CanDismiss { get; set; } = false;

    public ObservableCollection<ActivityActionViewModel> Actions { get; }

    public ActivityViewModel(OperationSnapshot snapshot)
    {
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
        Title = snapshot.Properties.Title;
        Message = snapshot.Properties.Message;
        Severity = snapshot.Properties.Severity;
        CanDismiss = IsDismissable(snapshot);
        UpdateActions(snapshot.Properties.Actions);
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
        foreach (var action in Actions)
        {
            if (!actionIds.Contains(action.Id))
            {
                Actions.Remove(action);
            }
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
    /// <param name="snapshot">The operation snapshot.</param>
    /// <returns>True if the activity can be dismissed; otherwise, false.</returns>
    private bool IsDismissable(OperationSnapshot snapshot)
    {
        return snapshot.Properties.Status == OperationStatus.Completed ||
               snapshot.Properties.Status == OperationStatus.Canceled;
    }
}

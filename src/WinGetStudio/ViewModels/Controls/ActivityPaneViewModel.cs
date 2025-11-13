// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Services.Operations.Contracts;
using WinGetStudio.Services.Operations.Models.States;

namespace WinGetStudio.ViewModels.Controls;

public partial class ActivityPaneViewModel : ObservableRecipient, IDisposable
{
    private readonly IOperationHub _operationHub;
    private readonly IDisposable _snapshotSubscription;
    private readonly IUIDispatcher _dispatcher;
    private bool _disposedValue;

    public ObservableCollection<ActivityViewModel> Activities { get; }

    public ActivityPaneViewModel(IOperationHub operationHub, IUIDispatcher dispatcher)
    {
        _operationHub = operationHub;
        _dispatcher = dispatcher;
        Activities = [];
        _snapshotSubscription = _operationHub.Snapshots.Subscribe(snapshots =>
        {
            _dispatcher.TryEnqueue(() => OnPublishOperationSnapshot(snapshots));
        });
    }

    /// <summary>
    /// Handles the publishing of operation snapshots.
    /// </summary>
    /// <param name="snapshots">The list of operation snapshots.</param>
    private void OnPublishOperationSnapshot(IReadOnlyList<OperationSnapshot> snapshots)
    {
        // Remove activities that are no longer present
        var snapshotIds = snapshots.Select(s => s.Id).ToHashSet();
        var toRemove = Activities.Where(a => !snapshotIds.Contains(a.Id)).ToList();
        foreach (var activity in toRemove)
        {
            Activities.Remove(activity);
        }

        // Reorder to match the snapshots order and update existing activities
        for (var i = 0; i < snapshots.Count; i++)
        {
            var snapshot = snapshots[i];
            var existingActivity = Activities.FirstOrDefault(a => a.Id == snapshot.Id);
            if (existingActivity != null)
            {
                existingActivity.Update(snapshot);
                var currentIndex = Activities.IndexOf(existingActivity);
                if (currentIndex != i)
                {
                    Activities.Move(currentIndex, i);
                }
            }
            else
            {
                Activities.Insert(i, new(snapshot));
            }
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _snapshotSubscription.Dispose();
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Management.Configuration;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Models;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;

namespace WinGetStudio.ViewModels;

public delegate ApplySetViewModel ApplySetViewModelFactory(IDSCSet applySet);

public sealed partial class ApplySetViewModel : ObservableObject, IDisposable
{
    private readonly ObservableCollection<ApplyUnitViewModel> _units;
    private readonly IStringLocalizer<ApplySetViewModel> _localizer;
    private readonly ILogger<ApplySetViewModel> _logger;
    private readonly IDSC _dsc;
    private readonly IDSCSet _applySet;
    private readonly IUIDispatcher _dispatcher;
    private CancellationTokenSource? _cts;
    private bool _disposedValue;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDone))]
    [NotifyPropertyChangedFor(nameof(CanCancel))]
    [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    public partial bool IsCompleted { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDone))]
    [NotifyPropertyChangedFor(nameof(CanCancel))]
    [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    public partial bool IsCanceled { get; set; }

    public bool IsDone => IsCompleted || IsCanceled;

    public bool CanCancel => !IsDone;

    public int TotalUnits => Units.Count;

    public int TotalCompletedUnits => Units.Count(u => u.IsCompleted);

    public string Summary => _localizer["ApplySet_TotalUnitsCompleted", TotalUnits == 0 ? 0 : (int)((double)TotalCompletedUnits / TotalUnits * 100)];

    public ReadOnlyObservableCollection<ApplyUnitViewModel> Units { get; }

    public ApplySetViewModel(
        IDSC dsc,
        IStringLocalizer<ApplySetViewModel> localizer,
        ILogger<ApplySetViewModel> logger,
        IUIDispatcher dispatcher,
        IDSCSet applySet)
    {
        _dsc = dsc;
        _localizer = localizer;
        _logger = logger;
        _applySet = applySet;
        _dispatcher = dispatcher;
        _cts = new();
        _units = [.. applySet.Units.Select(unit => new ApplyUnitViewModel(localizer, unit))];
        Units = new(_units);
    }

    /// <summary>
    /// Applies the configuration set asynchronously.
    /// </summary>
    /// <returns>The result of the apply operation.</returns>
    public async Task<IDSCApplySetResult> ApplyAsync()
    {
        try
        {
            _cts = new();
            var progress = new Progress<IDSCSetChangeData>(OnDataChanged);
            return await _dsc.ApplySetAsync(_applySet, progress, _cts.Token);
        }
        finally
        {
            _cts?.Dispose();
            _cts = null;
        }
    }

    /// <summary>
    /// Cancels the apply operation.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanCancel))]
    private async Task OnCancelAsync()
    {
        if (_cts != null && !_cts.IsCancellationRequested)
        {
            _logger.LogInformation("Cancellation requested");
            await _cts.CancelAsync();
            IsCanceled = true;
        }
    }

    /// <summary>
    /// Handles data change events.
    /// </summary>
    /// <param name="data">The change data.</param>
    private async void OnDataChanged(IDSCSetChangeData data)
    {
        await _dispatcher.EnqueueAsync(() => UpdateSetData(data));
    }

    /// <summary>
    /// Update the set based on change data.
    /// </summary>
    /// <param name="data">The change data.</param>
    private void UpdateSetData(IDSCSetChangeData data)
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

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _cts?.Dispose();
                _cts = null;
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

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
using WinGetStudio.Models.Operations;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using WinGetStudio.Services.Operations.Contracts;

namespace WinGetStudio.ViewModels;

public delegate ApplySetViewModel ApplySetViewModelFactory(IDSCSet applySet);

public sealed partial class ApplySetViewModel : ObservableObject
{
    private readonly ApplySetOperation _applySetOperation;
    private readonly ObservableCollection<ApplyUnitViewModel> _units;
    private readonly IStringLocalizer<ApplySetViewModel> _localizer;
    private readonly ILogger<ApplySetViewModel> _logger;
    private readonly IOperationFactory _operationFactory;
    private readonly IUIDispatcher _dispatcher;
    private IOperationContext? _context;

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
        IOperationFactory operationFactory,
        IStringLocalizer<ApplySetViewModel> localizer,
        ILogger<ApplySetViewModel> logger,
        IUIDispatcher dispatcher,
        IDSCSet applySet)
    {
        _operationFactory = operationFactory;
        _localizer = localizer;
        _logger = logger;
        _dispatcher = dispatcher;

        var progress = new Progress<IDSCSetChangeData>(OnDataChanged);
        _applySetOperation = _operationFactory.CreateApplySetOperation(applySet, progress);
        _units = [.. applySet.Units.Select(unit => new ApplyUnitViewModel(localizer, unit))];
        Units = new(_units);
    }

    /// <summary>
    /// Applies the configuration set asynchronously.
    /// </summary>
    /// <param name="context">The operation context.</param>
    /// <returns>The result of the apply operation.</returns>
    public async Task<OperationResult<IDSCApplySetResult>> ApplyAsync(IOperationContext context)
    {
        try
        {
            _context = context;
            return await _applySetOperation.ExecuteAsync(context);
        }
        finally
        {
            _context = null;
        }
    }

    /// <summary>
    /// Cancels the apply operation.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanCancel))]
    private void OnCancel()
    {
        if (_context != null)
        {
            _logger.LogInformation("Cancellation requested");
            _context.RequestCancellation();
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
}

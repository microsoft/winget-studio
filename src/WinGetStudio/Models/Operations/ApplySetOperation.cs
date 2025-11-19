// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Management.Configuration;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Exceptions;
using WinGetStudio.Services.Operations.Contracts;
using WinGetStudio.Services.Operations.Extensions;

namespace WinGetStudio.Models.Operations;

public sealed partial class ApplySetOperation : IOperation<OperationResult<IDSCApplySetResult>>
{
    private readonly ILogger<ApplySetOperation> _logger;
    private readonly IDSC _dsc;
    private readonly IDSCSet _dscSet;
    private readonly IStringLocalizer _localizer;
    private readonly IProgress<IDSCSetChangeData> _progress;
    private readonly ConcurrentDictionary<Guid, ConfigurationUnitState> _unitStates = [];

    private int TotalUnits => _dscSet.Units.Count;

    private int TotalCompletedUnits => _unitStates.Count(kvp => kvp.Value == ConfigurationUnitState.Completed);

    private int PercentComplete => TotalUnits == 0 ? 0 : (int)((double)TotalCompletedUnits / TotalUnits * 100);

    public ApplySetOperation(
        ILogger<ApplySetOperation> logger,
        IStringLocalizer localizer,
        IDSC dsc,
        IDSCSet dscSet,
        IProgress<IDSCSetChangeData> progress)
    {
        _logger = logger;
        _localizer = localizer;
        _dsc = dsc;
        _dscSet = dscSet;
        _progress = progress;
    }

    /// <inheritdoc/>
    public async Task<OperationResult<IDSCApplySetResult>> ExecuteAsync(IOperationContext context)
    {
        try
        {
            _logger.LogInformation($"Starting {nameof(ApplySetOperation)} operation with ID {context.Id}.");
            context.CommitSnapshot(props => props with { Message = _localizer["ApplySetOperation_StartMessage"] });
            var progress = new Progress<IDSCSetChangeData>(data => OnDataChanged(data, context));
            var result = await _dsc.ApplySetAsync(_dscSet, progress, context.CancellationToken);
            context.Complete(props => props with { Message = _localizer["ApplySetOperation_CompletedMessage"] });
            return new() { Result = result };
        }
        catch (OpenConfigurationSetException ex)
        {
            _logger.LogError(ex, $"Error opening configuration set");
            context.Fail(props => props with { Message = ex.GetErrorMessage(_localizer) });
            return new() { Error = ex };
        }
        catch (ApplyConfigurationSetException ex)
        {
            _logger.LogError(ex, $"Error applying configuration set");
            context.Fail(props => props with { Message = ex.GetSetErrorMessage(_localizer) });
            return new() { Error = ex };
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogInformation("Operation was canceled by user.");
            context.Canceled(props => props with { Message = _localizer["ApplySetOperation_CancelledMessage"] });
            return new() { Error = ex };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error during {nameof(ApplySetOperation)} operation.");
            context.Fail(props => props with { Message = _localizer["ApplySetOperation_UnexpectedErrorMessage", ex.Message] });
            return new() { Error = ex };
        }
    }

    /// <summary>
    /// Handler for data changed events from the DSC apply operation.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <param name="context">The operation context.</param>
    private void OnDataChanged(IDSCSetChangeData data, IOperationContext context)
    {
        if (data.Change == ConfigurationSetChangeEventType.UnitStateChanged &&
            data.Unit != null &&
            _unitStates.ContainsKey(data.Unit.InstanceId))
        {
            _unitStates[data.Unit.InstanceId] = data.UnitState;
            context.ReportProgress(PercentComplete);
        }

        // Report the data to the original progress reporter
        _progress.Report(data);
    }
}

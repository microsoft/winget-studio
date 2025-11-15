// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
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
            context.Start();
            context.StartSnapshotBroadcast();
            _logger.LogInformation($"Applying configuration set started");
            var result = await _dsc.ApplySetAsync(_dscSet, _progress, context.CancellationToken);
            return new() { Result = result };
        }
        catch (OpenConfigurationSetException ex)
        {
            _logger.LogError(ex, $"Opening configuration set failed during apply");
            context.Fail(props => props with { Message = ex.GetErrorMessage(_localizer) });
            return new() { Error = ex };
        }
        catch (ApplyConfigurationSetException ex)
        {
            _logger.LogError(ex, $"Applying configuration set failed");
            context.Fail(props => props with { Message = ex.GetSetErrorMessage(_localizer) });
            return new() { Error = ex };
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogInformation("Applying configuration set was canceled by user");
            context.Canceled(props => props with { Message = _localizer["ApplyFile_ApplyOperationCanceledMessage"] });
            return new() { Error = ex };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unknown error while validating configuration code");
            context.Fail(props => props with { Message = ex.Message });
            return new() { Error = ex };
        }
    }
}

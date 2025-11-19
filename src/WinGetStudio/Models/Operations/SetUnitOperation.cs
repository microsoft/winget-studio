// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Exceptions;
using WinGetStudio.Services.Operations.Contracts;
using WinGetStudio.Services.Operations.Extensions;

namespace WinGetStudio.Models.Operations;

public sealed partial class SetUnitOperation : IOperation<OperationResult<IDSCApplyUnitResult>>
{
    private readonly ILogger<SetUnitOperation> _logger;
    private readonly IDSC _dsc;
    private readonly IDSCFile _dscFile;
    private readonly IStringLocalizer _localizer;

    public SetUnitOperation(ILogger<SetUnitOperation> logger, IStringLocalizer localizer, IDSC dsc, IDSCFile dscFile)
    {
        _logger = logger;
        _localizer = localizer;
        _dsc = dsc;
        _dscFile = dscFile;
    }

    /// <inheritdoc/>
    public async Task<OperationResult<IDSCApplyUnitResult>> ExecuteAsync(IOperationContext context)
    {
        try
        {
            _logger.LogInformation($"Starting {nameof(SetUnitOperation)} operation with ID {context.Id}.");
            context.CommitSnapshot(props => props with { Message = _localizer["SetUnitOperation_StartMessage"] });
            var dscSet = await _dsc.OpenConfigurationSetAsync(_dscFile);
            var dscUnit = dscSet.Units[0];
            var result = await _dsc.SetUnitAsync(dscUnit, context.CancellationToken);
            var resultInfo = result.ResultInformation;
            if (resultInfo != null && !resultInfo.IsOk)
            {
                context.Fail(props => props with { Message = GetDescriptiveFailureMessage(resultInfo) });
            }
            else
            {
                context.Success(props => props with { Message = _localizer["SetUnitOperation_SuccessMessage"] });
            }

            return new() { Result = result };
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogInformation("Operation was canceled by user.");
            context.Canceled(props => props with { Message = _localizer["SetUnitOperation_CancelledMessage"] });
            return new() { Error = ex };
        }
        catch (OpenConfigurationSetException ex)
        {
            _logger.LogError(ex, $"Error opening configuration set");
            context.Fail(props => props with { Message = ex.GetErrorMessage(_localizer) });
            return new() { Error = ex };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error during {nameof(SetUnitOperation)} operation.");
            context.Fail(props => props with { Message = _localizer["SetUnitOperation_UnexpectedErrorMessage", ex.Message] });
            return new() { Error = ex };
        }
    }

    private string GetDescriptiveFailureMessage(IDSCUnitResultInformation resultInfo)
    {
        var message = $"0x{resultInfo.ResultCode.HResult:X}";
        var info = new[] { resultInfo.Description, resultInfo.Details }.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
        if (info.Length == 0)
        {
            return message;
        }

        if (info.Length == 1)
        {
            return $"{message}: {info[0]}";
        }

        return $"{message}: {string.Join(Environment.NewLine, info)}";
    }
}

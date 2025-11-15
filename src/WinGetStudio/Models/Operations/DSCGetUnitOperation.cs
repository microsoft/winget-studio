// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Exceptions;
using WinGetStudio.Services.Operations.Contracts;
using WinGetStudio.Services.Operations.Extensions;

namespace WinGetStudio.Models.Operations;

public sealed partial class DSCGetUnitOperation : IOperation<DSCOperationResult<IDSCGetUnitResult>>
{
    private readonly ILogger<DSCGetUnitOperation> _logger;
    private readonly IDSC _dsc;
    private readonly IDSCFile _dscFile;
    private readonly IStringLocalizer _localizer;

    public DSCGetUnitOperation(ILogger<DSCGetUnitOperation> logger, IStringLocalizer localizer, IDSC dsc, IDSCFile dscFile)
    {
        _logger = logger;
        _localizer = localizer;
        _dsc = dsc;
        _dscFile = dscFile;
    }

    /// <inheritdoc/>
    public async Task<DSCOperationResult<IDSCGetUnitResult>> ExecuteAsync(IOperationContext context)
    {
        try
        {
            context.Start();
            context.StartSnapshotBroadcast();
            context.AddCancelAction("Cancel");
            var dscSet = await _dsc.OpenConfigurationSetAsync(_dscFile);
            var dscUnit = dscSet.Units[0];
            var result = await _dsc.GetUnitAsync(dscUnit, context.CancellationToken);
            var resultInfo = result.ResultInformation;
            if (resultInfo != null && !resultInfo.IsOk)
            {
                var title = $"0x{resultInfo.ResultCode.HResult:X}";
                List<string> messageList = [resultInfo.Description, resultInfo.Details];
                var message = string.Join(Environment.NewLine, messageList.Where(s => !string.IsNullOrEmpty(s)));
                context.Fail(props => props with { Title = title, Message = message });
            }

            return new(result);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "The DSC get unit operation was canceled.");
            context.Canceled(props => props with { Message = "The operation was canceled." });
            return new(ex);
        }
        catch (OpenConfigurationSetException ex)
        {
            _logger.LogError(ex, "An error occurred while opening the DSC configuration set.");
            context.Fail(props => props with { Message = ex.GetErrorMessage(_localizer) });
            return new(ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while executing a DSC operation.");
            context.Fail(props => props with { Message = ex.Message });
            return new(ex);
        }
    }
}

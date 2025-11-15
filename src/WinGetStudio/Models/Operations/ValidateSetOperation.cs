// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Exceptions;
using WinGetStudio.Services.Operations.Contracts;
using WinGetStudio.Services.Operations.Extensions;

namespace WinGetStudio.Models.Operations;

public sealed partial class ValidateSetOperation : IOperation<OperationResult<IDSCApplySetResult>>
{
    private readonly ILogger<ValidateSetOperation> _logger;
    private readonly IDSC _dsc;
    private readonly IDSCFile _dscFile;
    private readonly IStringLocalizer _localizer;

    public ValidateSetOperation(ILogger<ValidateSetOperation> logger, IStringLocalizer localizer, IDSC dsc, IDSCFile dscFile)
    {
        _logger = logger;
        _localizer = localizer;
        _dsc = dsc;
        _dscFile = dscFile;
    }

    /// <inheritdoc/>
    public async Task<OperationResult<IDSCApplySetResult>> ExecuteAsync(IOperationContext context)
    {
        try
        {
            context.Start();
            context.StartSnapshotBroadcast();
            _logger.LogInformation($"Validating configuration code");
            var dscSet = await _dsc.OpenConfigurationSetAsync(_dscFile);
            var result = await _dsc.ValidateSetAsync(dscSet);
            context.Success(props => props with { Message = _localizer["PreviewFile_ValidationSuccessfulMessage"] });
            return new() { Result = result };
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "The DSC validation operation was canceled.");
            context.Canceled(props => props with { Message = "The operation was canceled." });
            return new() { Error = ex };
        }
        catch (OpenConfigurationSetException ex)
        {
            _logger.LogError(ex, "An error occurred while opening the DSC configuration set.");
            context.Fail(props => props with { Message = ex.GetErrorMessage(_localizer) });
            return new() { Error = ex };
        }
        catch (ApplyConfigurationSetException ex)
        {
            _logger.LogError(ex, $"Validation of configuration set failed");
            var title = ex.GetSetErrorMessage(_localizer);
            var message = ex.GetUnitsSummaryMessage(_localizer);
            context.Fail(props => props with { Title = title, Message = message });
            return new() { Error = ex };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while executing a DSC operation.");
            context.Fail(props => props with { Message = ex.Message });
            return new() { Error = ex };
        }
    }
}

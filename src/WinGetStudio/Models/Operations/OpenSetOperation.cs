// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Exceptions;
using WinGetStudio.Services.Operations.Contracts;
using WinGetStudio.Services.Operations.Extensions;

namespace WinGetStudio.Models.Operations;

public sealed partial class OpenSetOperation : IOperation<OperationResult<IDSCSet>>
{
    private readonly ILogger<OpenSetOperation> _logger;
    private readonly IDSC _dsc;
    private readonly IDSCFile _dscFile;
    private readonly IStringLocalizer _localizer;

    public OpenSetOperation(ILogger<OpenSetOperation> logger, IStringLocalizer localizer, IDSC dsc, IDSCFile dscFile)
    {
        _logger = logger;
        _localizer = localizer;
        _dsc = dsc;
        _dscFile = dscFile;
    }

    /// <inheritdoc/>
    public async Task<OperationResult<IDSCSet>> ExecuteAsync(IOperationContext context)
    {
        try
        {
            _logger.LogInformation($"Starting {nameof(OpenSetOperation)} operation with ID {context.Id}.");
            context.CommitSnapshot(props => props with { Message = _localizer["OpenSetOperation_StartMessage"] });
            var result = await _dsc.OpenConfigurationSetAsync(_dscFile, context.CancellationToken);
            context.Success(props => props with { Message = _localizer["OpenSetOperation_SuccessMessage"] });
            return new() { Result = result };
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogInformation("Operation was canceled by user.");
            context.Canceled(props => props with { Message = _localizer["OpenSetOperation_CancelledMessage"] });
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
            _logger.LogError(ex, $"Unexpected error during {nameof(OpenSetOperation)} operation.");
            context.Fail(props => props with { Message = _localizer["OpenSetOperation_UnexpectedErrorMessage", ex.Message] });
            return new() { Error = ex };
        }
    }
}

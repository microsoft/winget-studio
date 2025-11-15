// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Exceptions;
using WinGetStudio.Services.Operations.Contracts;
using WinGetStudio.Services.Operations.Extensions;

namespace WinGetStudio.Models.Operations;

public sealed partial class DSCOpenSetOperation : IOperation<DSCOperationResult<IDSCSet>>
{
    private readonly ILogger<DSCOpenSetOperation> _logger;
    private readonly IDSC _dsc;
    private readonly IDSCFile _dscFile;
    private readonly IStringLocalizer _localizer;

    public DSCOpenSetOperation(ILogger<DSCOpenSetOperation> logger, IStringLocalizer localizer, IDSC dsc, IDSCFile dscFile)
    {
        _logger = logger;
        _localizer = localizer;
        _dsc = dsc;
        _dscFile = dscFile;
    }

    /// <inheritdoc/>
    public async Task<DSCOperationResult<IDSCSet>> ExecuteAsync(IOperationContext context)
    {
        try
        {
            context.Start();
            context.StartSnapshotBroadcast();
            context.AddCancelAction("Cancel");
            var result = await _dsc.OpenConfigurationSetAsync(_dscFile, context.CancellationToken);
            return new(result);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "Opening the DSC configuration set was canceled.");
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

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Management.Configuration;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Exceptions;
using WinGetStudio.Services.Operations.Contracts;
using WinGetStudio.Services.Operations.Extensions;

namespace WinGetStudio.Models.Operations;

public sealed partial class DSCTestSetOperation : IOperation<DSCOperationResult<IDSCTestSetResult>>
{
    private readonly ILogger<DSCTestSetOperation> _logger;
    private readonly IDSC _dsc;
    private readonly IDSCFile _dscFile;
    private readonly IStringLocalizer _localizer;

    public DSCTestSetOperation(ILogger<DSCTestSetOperation> logger, IStringLocalizer localizer, IDSC dsc, IDSCFile dscFile)
    {
        _logger = logger;
        _localizer = localizer;
        _dsc = dsc;
        _dscFile = dscFile;
    }

    /// <inheritdoc/>
    public async Task<DSCOperationResult<IDSCTestSetResult>> ExecuteAsync(IOperationContext context)
    {
        try
        {
            context.Start();
            context.StartSnapshotBroadcast();
            _logger.LogInformation($"Testing configuration code");
            var dscSet = await _dsc.OpenConfigurationSetAsync(_dscFile, context.CancellationToken);

            // TODO capture progress and pass CT
            var result = await _dsc.TestSetAsync(dscSet);
            if (result.TestResult == ConfigurationTestResult.Positive)
            {
                context.Success(props => props with { Message = _localizer["Notification_MachineInDesiredState"] });
            }
            else
            {
                context.Fail(props => props with { Message = _localizer["Notification_MachineNotInDesiredState"] });
            }

            return new(result);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "The DSC test set operation was canceled.");
            context.Canceled(props => props with { Message = "The operation was canceled." });
            return new(ex);
        }
        catch (OpenConfigurationSetException ex)
        {
            _logger.LogError(ex, $"Opening configuration set failed during validation");
            context.Fail(props => props with { Message = ex.GetErrorMessage(_localizer) });
            return new(ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unknown error while validating configuration code");
            context.Fail(props => props with { Message = ex.Message });
            return new(ex);
        }
    }
}

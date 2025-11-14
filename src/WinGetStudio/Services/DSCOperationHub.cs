// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Models.Operations;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using WinGetStudio.Services.Operations.Contracts;
using WinGetStudio.Services.Operations.Models;
using WinGetStudio.Services.Operations.Models.Policies;
using WinGetStudio.Services.Operations.Models.States;

namespace WinGetStudio.Services;

public sealed partial class DSCOperationHub : IDSCOperationHub
{
    private readonly IOperationHub _operationHub;
    private readonly IDSC _dsc;
    private readonly ILogger<DSCOperationHub> _logger;
    private readonly IStringLocalizer<DSCOperationHub> _localizer;
    private readonly OperationExecutionOptions _options;

    public DSCOperationHub(
        IOperationHub operationHub,
        IDSC dsc,
        ILogger<DSCOperationHub> logger,
        IStringLocalizer<DSCOperationHub> localizer)
    {
        _operationHub = operationHub;
        _dsc = dsc;
        _logger = logger;
        _localizer = localizer;
        _options = new()
        {
            Policies =
            [
                new AutoCompletePolicy(),
                new SnapshotRetentionPolicy(OperationStatus.Completed, OperationSeverity.Success, TimeSpan.FromSeconds(3))
            ],
        };
    }

    /// <inheritdoc/>
    public async Task<DSCOperationResult<IDSCGetUnitResult>> ExecuteGetUnitAsync(IDSCFile dscFile, CancellationToken cancellationToken = default)
    {
        var operation = new DSCGetUnitOperation(_logger, _localizer, _dsc, dscFile);
        return await _operationHub.ExecuteAsync(operation, _options, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DSCOperationResult<IDSCApplyUnitResult>> ExecuteSetUnitAsync(IDSCFile dscFile, CancellationToken cancellationToken = default)
    {
        var operation = new DSCSetUnitOperation(_logger, _localizer, _dsc, dscFile);
        return await _operationHub.ExecuteAsync(operation, _options, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DSCOperationResult<IDSCTestUnitResult>> ExecuteTestUnitAsync(IDSCFile dscFile, CancellationToken cancellationToken = default)
    {
        var operation = new DSCTestUnitOperation(_logger, _localizer, _dsc, dscFile);
        return await _operationHub.ExecuteAsync(operation, _options, cancellationToken);
    }
}

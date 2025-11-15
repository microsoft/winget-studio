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
    private readonly ILoggerFactory _loggerFactory;
    private readonly IStringLocalizer<DSCOperationHub> _localizer;
    private readonly OperationExecutionOptions _interactiveOperationOptions;
    private readonly OperationExecutionOptions _passiveOperationOptions;

    public DSCOperationHub(
        IOperationHub operationHub,
        IDSC dsc,
        ILoggerFactory loggerFactory,
        IStringLocalizer<DSCOperationHub> localizer)
    {
        _operationHub = operationHub;
        _dsc = dsc;
        _loggerFactory = loggerFactory;
        _localizer = localizer;

        _interactiveOperationOptions = new()
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
        var logger = _loggerFactory.CreateLogger<DSCGetUnitOperation>();
        var operation = new DSCGetUnitOperation(logger, _localizer, _dsc, dscFile);
        return await ExecuteInteractiveOperationAsync(operation, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DSCOperationResult<IDSCApplyUnitResult>> ExecuteSetUnitAsync(IDSCFile dscFile, CancellationToken cancellationToken = default)
    {
        var logger = _loggerFactory.CreateLogger<DSCSetUnitOperation>();
        var operation = new DSCSetUnitOperation(logger, _localizer, _dsc, dscFile);
        return await ExecuteInteractiveOperationAsync(operation, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DSCOperationResult<IDSCTestUnitResult>> ExecuteTestUnitAsync(IDSCFile dscFile, CancellationToken cancellationToken = default)
    {
        var logger = _loggerFactory.CreateLogger<DSCTestUnitOperation>();
        var operation = new DSCTestUnitOperation(logger, _localizer, _dsc, dscFile);
        return await ExecuteInteractiveOperationAsync(operation, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DSCOperationResult<IDSCApplySetResult>> ExecuteValidateSetAsync(IDSCFile dscFile, CancellationToken cancellationToken = default)
    {
        var logger = _loggerFactory.CreateLogger<DSCValidateSetOperation>();
        var operation = new DSCValidateSetOperation(logger, _localizer, _dsc, dscFile);
        return await ExecuteInteractiveOperationAsync(operation, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DSCOperationResult<IDSCTestSetResult>> ExecuteTestSetAsync(IDSCFile dscFile, CancellationToken cancellationToken = default)
    {
        var logger = _loggerFactory.CreateLogger<DSCTestSetOperation>();
        var operation = new DSCTestSetOperation(logger, _localizer, _dsc, dscFile);
        return await ExecuteInteractiveOperationAsync(operation, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DSCOperationResult<IDSCSet>> ExecuteOpenSetAsync(IDSCFile dscFile, CancellationToken cancellationToken = default)
    {
        var logger = _loggerFactory.CreateLogger<DSCOpenSetOperation>();
        var operation = new DSCOpenSetOperation(logger, _localizer, _dsc, dscFile);
        return await ExecuteInteractiveOperationAsync(operation, cancellationToken);
    }

    /// <summary>
    /// Execute an interactive operation.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="operation">The operation.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The operation result.</returns>
    private Task<TResult> ExecuteInteractiveOperationAsync<TResult>(IOperation<TResult> operation, CancellationToken cancellationToken = default)
    {
        return _operationHub.ExecuteAsync(operation, _interactiveOperationOptions, cancellationToken);
    }

    /// <summary>
    /// Execute a passive operation.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="operation">The operation.</param>
    /// <returns>The operation result.</returns>
    private Task<T> ExecutePassiveOperationAsync<T>(IOperation<T> operation)
    {
        return _operationHub.ExecuteAsync(operation, _passiveOperationOptions);
    }
}

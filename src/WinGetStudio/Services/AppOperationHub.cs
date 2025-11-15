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

public sealed partial class AppOperationHub : IAppOperationHub
{
    private readonly IOperationHub _operationHub;
    private readonly IDSC _dsc;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger _logger;
    private readonly IStringLocalizer<AppOperationHub> _localizer;
    private readonly OperationExecutionOptions _interactiveOperationOptions;
    private readonly OperationExecutionOptions _passiveOperationOptions;

    public AppOperationHub(
        IOperationHub operationHub,
        IDSC dsc,
        ILogger<AppOperationHub> logger,
        ILoggerFactory loggerFactory,
        IStringLocalizer<AppOperationHub> localizer)
    {
        _operationHub = operationHub;
        _dsc = dsc;
        _logger = logger;
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
        _passiveOperationOptions = new();
    }

    /// <inheritdoc/>
    public async Task<OperationResult<IDSCGetUnitResult>> ExecuteGetUnitAsync(IDSCFile dscFile, CancellationToken cancellationToken = default)
    {
        var logger = _loggerFactory.CreateLogger<GetUnitOperation>();
        var operation = new GetUnitOperation(logger, _localizer, _dsc, dscFile);
        return await ExecuteInteractiveOperationAsync(operation, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<OperationResult<IDSCApplyUnitResult>> ExecuteSetUnitAsync(IDSCFile dscFile, CancellationToken cancellationToken = default)
    {
        var logger = _loggerFactory.CreateLogger<SetUnitOperation>();
        var operation = new SetUnitOperation(logger, _localizer, _dsc, dscFile);
        return await ExecuteInteractiveOperationAsync(operation, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<OperationResult<IDSCTestUnitResult>> ExecuteTestUnitAsync(IDSCFile dscFile, CancellationToken cancellationToken = default)
    {
        var logger = _loggerFactory.CreateLogger<TestUnitOperation>();
        var operation = new TestUnitOperation(logger, _localizer, _dsc, dscFile);
        return await ExecuteInteractiveOperationAsync(operation, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<OperationResult<IDSCApplySetResult>> ExecuteValidateSetAsync(IDSCFile dscFile, CancellationToken cancellationToken = default)
    {
        var logger = _loggerFactory.CreateLogger<ValidateSetOperation>();
        var operation = new ValidateSetOperation(logger, _localizer, _dsc, dscFile);
        return await ExecuteInteractiveOperationAsync(operation, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<OperationResult<IDSCApplySetResult>> ExecuteApplySetAsync(IDSCSet dscSet, IProgress<IDSCSetChangeData> progress, CancellationToken cancellationToken = default)
    {
        var logger = _loggerFactory.CreateLogger<ApplySetOperation>();
        var operation = new ApplySetOperation(logger, _localizer, _dsc, dscSet, progress);
        return await ExecuteInteractiveOperationAsync(operation, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<OperationResult<IDSCTestSetResult>> ExecuteTestSetAsync(IDSCFile dscFile, CancellationToken cancellationToken = default)
    {
        var logger = _loggerFactory.CreateLogger<TestSetOperation>();
        var operation = new TestSetOperation(logger, _localizer, _dsc, dscFile);
        return await ExecuteInteractiveOperationAsync(operation, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<OperationResult<IDSCSet>> ExecuteOpenSetAsync(IDSCFile dscFile, CancellationToken cancellationToken = default)
    {
        var logger = _loggerFactory.CreateLogger<OpenSetOperation>();
        var operation = new OpenSetOperation(logger, _localizer, _dsc, dscFile);
        return await ExecuteInteractiveOperationAsync(operation, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<OperationResult<bool>> ExecuteSaveDscFile(IDSCFile dscFile)
    {
        var logger = _loggerFactory.CreateLogger<SaveDSCFileOperation>();
        var operation = new SaveDSCFileOperation(logger, _localizer, dscFile);
        return await ExecutePassiveOperationAsync(operation);
    }

    /// <summary>
    /// Execute a passive operation.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="operation">The operation.</param>
    /// <returns>The operation result.</returns>
    public async Task<OperationResult<TResult>> ExecutePassiveOperationAsync<TResult>(Func<IOperationContext, Task<OperationResult<TResult>>> operation)
    {
        try
        {
            return await _operationHub.ExecuteAsync(operation, _passiveOperationOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled error occurred while executing a passive operation.");
            return new() { Error = ex };
        }
    }

    /// <summary>
    /// Execute a passive operation.
    /// </summary>
    /// <param name="operation">The operation.</param>
    public async Task ExecutePassiveOperationAsync(Func<IOperationContext, Task> operation)
    {
        try
        {
            await _operationHub.ExecuteAsync(operation, _passiveOperationOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled error occurred while executing a passive operation.");
        }
    }

    /// <summary>
    /// Execute a passive operation.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="operation">The operation.</param>
    /// <returns>The operation result.</returns>
    private async Task<OperationResult<TResult>> ExecutePassiveOperationAsync<TResult>(IOperation<OperationResult<TResult>> operation)
    {
        try
        {
            return await _operationHub.ExecuteAsync(operation, _passiveOperationOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled error occurred while executing a passive operation.");
            return new() { Error = ex };
        }
    }

    /// <summary>
    /// Execute an interactive operation.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="operation">The operation.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The operation result.</returns>
    private async Task<OperationResult<TResult>> ExecuteInteractiveOperationAsync<TResult>(IOperation<OperationResult<TResult>> operation, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _operationHub.ExecuteAsync(operation, _interactiveOperationOptions, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled error occurred while executing an interactive operation.");
            return new() { Error = ex };
        }
    }
}

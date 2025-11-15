// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.Models.Operations;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using WinGetStudio.Services.Operations.Contracts;

namespace WinGetStudio.Contracts.Services;

public interface IAppOperationHub
{
    /// <summary>
    /// Executes a Get operation for the specified DSC Unit.
    /// </summary>
    /// <param name="dscFile">The DSC File to get the unit from.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the Get operation.</returns>
    Task<OperationResult<IDSCGetUnitResult>> ExecuteGetUnitAsync(IDSCFile dscFile, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a Set operation for the specified DSC Unit.
    /// </summary>
    /// <param name="dscFile">The DSC File to set the unit from.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the Set operation.</returns>
    Task<OperationResult<IDSCApplyUnitResult>> ExecuteSetUnitAsync(IDSCFile dscFile, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a Test operation for the specified DSC Unit.
    /// </summary>
    /// <param name="dscFile">The DSC File to test the unit from.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the Test operation.</returns>
    Task<OperationResult<IDSCTestUnitResult>> ExecuteTestUnitAsync(IDSCFile dscFile, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a Validate operation for the specified DSC Configuration Set.
    /// </summary>
    /// <param name="dscFile">The DSC File to validate the configuration set from.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the Validate operation.</returns>
    Task<OperationResult<IDSCApplySetResult>> ExecuteValidateSetAsync(IDSCFile dscFile, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an Apply operation for the specified DSC Configuration Set.
    /// </summary>
    /// <param name="dscSet">The DSC Configuration Set to apply.</param>
    /// <param name="progress">The progress reporter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the Apply operation.</returns>
    Task<OperationResult<IDSCApplySetResult>> ExecuteApplySetAsync(IDSCSet dscSet, IProgress<IDSCSetChangeData> progress, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a Test operation for the specified DSC Configuration Set.
    /// </summary>
    /// <param name="dscFile">The DSC File to test the configuration set from.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the Test operation.</returns>
    Task<OperationResult<IDSCTestSetResult>> ExecuteTestSetAsync(IDSCFile dscFile, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an Open operation for the specified DSC Configuration Set.
    /// </summary>
    /// <param name="dscFile">The DSC File to open the configuration set from.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the Open operation.</returns>
    Task<OperationResult<IDSCSet>> ExecuteOpenSetAsync(IDSCFile dscFile, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a Save operation for the specified DSC File.
    /// </summary>
    /// <param name="dscFile">The DSC File to save.</param>
    /// <returns>The result of the Save operation.</returns>
    Task<OperationResult<bool>> ExecuteSaveDscFile(IDSCFile dscFile);

    /// <summary>
    /// Execute a passive operation.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="operation">The operation.</param>
    /// <returns>The operation result.</returns>
    Task<OperationResult<TResult>> ExecutePassiveOperationAsync<TResult>(Func<IOperationContext, Task<OperationResult<TResult>>> operation);

    /// <summary>
    /// Execute a passive operation.
    /// </summary>
    /// <param name="operation">The operation.</param>
    Task ExecutePassiveOperationAsync(Func<IOperationContext, Task> operation);
}

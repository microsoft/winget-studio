// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.Models.Operations;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;

namespace WinGetStudio.Contracts.Services;

public interface IDSCOperationHub
{
    /// <summary>
    /// Executes a Get operation for the specified DSC Unit.
    /// </summary>
    /// <param name="dscFile">The DSC File to get the unit from.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the Get operation.</returns>
    Task<DSCOperationResult<IDSCGetUnitResult>> ExecuteGetUnitAsync(IDSCFile dscFile, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a Set operation for the specified DSC Unit.
    /// </summary>
    /// <param name="dscFile">The DSC File to set the unit from.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the Set operation.</returns>
    Task<DSCOperationResult<IDSCApplyUnitResult>> ExecuteSetUnitAsync(IDSCFile dscFile, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a Test operation for the specified DSC Unit.
    /// </summary>
    /// <param name="dscFile">The DSC File to test the unit from.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>>The result of the Test operation.</returns>
    Task<DSCOperationResult<IDSCTestUnitResult>> ExecuteTestUnitAsync(IDSCFile dscFile, CancellationToken cancellationToken = default);
}

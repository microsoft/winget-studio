// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using WinGetStudio.Services.DesiredStateConfiguration.Models;

namespace WinGetStudio.Services.DesiredStateConfiguration.Contracts;

internal interface IDSCOperations
{
    /// <summary>
    /// Open a DSC configuration set from a file
    /// </summary>
    /// <param name="file">Configuration file to open</param>
    /// <returns>Configuration set</returns>
    public Task<IDSCSet> OpenConfigurationSetAsync(IDSCFile file);

    /// <summary>
    /// Validate a DSC configuration set
    /// </summary>
    /// <param name="set">Configuration set to validate</param>
    /// <returns>Result of validating the configuration</returns>
    public IAsyncOperationWithProgress<IDSCApplySetResult, IDSCSetChangeData> ValidateSetAsync(IDSCSet set);

    /// <summary>
    /// Apply a DSC configuration set
    /// </summary>
    /// <param name="set">Configuration set to apply</param>
    /// <returns>Result of applying the configuration</returns>
    public IAsyncOperationWithProgress<IDSCApplySetResult, IDSCSetChangeData> ApplySetAsync(IDSCSet set);

    /// <summary>
    /// Test a DSC configuration set
    /// </summary>
    /// <param name="inputSet">Configuration set to test</param>
    /// <returns>Result of testing the configuration</returns>
    public IAsyncOperationWithProgress<IDSCTestSetResult, IDSCTestUnitResult> TestSetAsync(IDSCSet inputSet);

    /// <summary>
    /// Gets the details for all configuration units in the specified set.
    /// </summary>
    /// <param name="set">Configuration set to get details for</param>
    /// <param name="progress">Progress callback</param>
    /// <param name="ct">Cancellation token</param>
    Task<IDSCGetSetDetailsResult> GetSetDetailsAsync(IDSCSet set, IProgress<IDSCGetUnitDetailsResult> progress = null, CancellationToken ct = default);

    /// <summary>
    /// Gets the details for the specified configuration unit.
    /// </summary>
    /// <param name="unit">Unit to get details for</param>
    /// <param name="ct">Cancellation token</param>
    Task<IDSCGetUnitDetailsResult> GetUnitDetailsAsync(IDSCUnit unit, CancellationToken ct = default);

    /// <summary>
    /// Get details for the specified configuration unit.
    /// </summary>
    /// <param name="unit">Unit to get details for</param>
    /// <returns>Result of getting the unit details</returns>
    public Task<IDSCGetUnitResult> GetUnitAsync(IDSCUnit unit);

    /// <summary>
    /// Set the machine state to the specified configuration unit.
    /// </summary>
    /// <param name="unit">Unit to set</param>
    /// <returns>Result of setting the unit</returns>
    public Task<IDSCApplyUnitResult> SetUnitAsync(IDSCUnit unit);

    /// <summary>
    /// Test whether the current machine state is the same as the configuration unit.
    /// </summary>
    /// <param name="unit">Unit to test</param>
    /// <returns>Result of testing the unit</returns>
    public Task<IDSCTestUnitResult> TestUnitAsync(IDSCUnit unit);

    /// <summary>
    /// Gets the list of available DSC v3 resources on the system.
    /// </summary>
    /// <returns>List of available DSC v3 resources.</returns>
    public Task<IReadOnlyList<ResourceMetada>> GetDscV3ResourcesAsync();
}

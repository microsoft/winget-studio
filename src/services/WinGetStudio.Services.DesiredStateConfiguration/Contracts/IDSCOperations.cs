// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WinGetStudio.Services.DesiredStateConfiguration.Models;

namespace WinGetStudio.Services.DesiredStateConfiguration.Contracts;

internal interface IDSCOperations
{
    /// <summary>
    /// Open a DSC configuration set from a file
    /// </summary>
    /// <param name="file">Configuration file to open</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Configuration set</returns>
    Task<IDSCSet> OpenConfigurationSetAsync(IDSCFile file, CancellationToken ct = default);

    /// <summary>
    /// Validate a DSC configuration set
    /// </summary>
    /// <param name="set">Configuration set to validate</param>
    /// <param name="progress">Progress callback</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Result of validating the configuration</returns>
    Task<IDSCApplySetResult> ValidateSetAsync(IDSCSet set, IProgress<IDSCSetChangeData> progress = null, CancellationToken ct = default);

    /// <summary>
    /// Apply a DSC configuration set
    /// </summary>
    /// <param name="set">Configuration set to apply</param>
    /// <param name="progress">Progress callback</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Result of applying the configuration</returns>
    Task<IDSCApplySetResult> ApplySetAsync(IDSCSet set, IProgress<IDSCSetChangeData> progress = null, CancellationToken ct = default);

    /// <summary>
    /// Test a DSC configuration set
    /// </summary>
    /// <param name="inputSet">Configuration set to test</param>
    /// <param name="progress">Progress callback</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Result of testing the configuration</returns>
    Task<IDSCTestSetResult> TestSetAsync(IDSCSet inputSet, IProgress<IDSCTestUnitResult> progress = null, CancellationToken ct = default);

    /// <summary>
    /// Get details of configuration units in a set
    /// </summary>
    /// <param name="set">Configuration set to get details for</param>
    void GetConfigurationUnitDetails(IDSCSet set);

    /// <summary>
    /// Get details for the specified configuration unit.
    /// </summary>
    /// <param name="unit">Unit to get details for</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Result of getting the unit details</returns>
    Task<IDSCGetUnitResult> GetUnitAsync(IDSCUnit unit, CancellationToken ct = default);

    /// <summary>
    /// Set the machine state to the specified configuration unit.
    /// </summary>
    /// <param name="unit">Unit to set</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Result of setting the unit</returns>
    Task<IDSCApplyUnitResult> SetUnitAsync(IDSCUnit unit, CancellationToken ct = default);

    /// <summary>
    /// Test whether the current machine state is the same as the configuration unit.
    /// </summary>
    /// <param name="unit">Unit to test</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Result of testing the unit</returns>
    Task<IDSCTestUnitResult> TestUnitAsync(IDSCUnit unit, CancellationToken ct = default);

    /// <summary>
    /// Gets the list of available DSC v3 resources on the system.
    /// </summary>
    /// <returns>List of available DSC v3 resources.</returns>
    Task<IReadOnlyList<ResourceMetada>> GetDscV3ResourcesAsync();
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Models;

namespace WinGetStudio.Services.DesiredStateConfiguration.Services;

internal sealed class DSC : IDSC
{
    private readonly IDSCDeployment _dscDeployment;
    private readonly IDSCOperations _dscOperations;

    public DSC(IDSCDeployment dscDeployment, IDSCOperations dscOperations)
    {
        _dscDeployment = dscDeployment;
        _dscOperations = dscOperations;
    }

    /// <inheritdoc/>
    public async Task<bool> IsUnstubbedAsync() => await _dscDeployment.IsUnstubbedAsync();

    /// <inheritdoc/>
    public async Task<bool> UnstubAsync() => await _dscDeployment.UnstubAsync();

    /// <inheritdoc/>
    public Task<IDSCSet> OpenConfigurationSetAsync(IDSCFile file, CancellationToken ct = default) => _dscOperations.OpenConfigurationSetAsync(file, ct);

    /// <inheritdoc/>
    public Task<IDSCApplySetResult> ValidateSetAsync(IDSCSet set, IProgress<IDSCSetChangeData> progress = null, CancellationToken ct = default) => _dscOperations.ValidateSetAsync(set, progress, ct);

    /// <inheritdoc/>
    public Task<IDSCApplySetResult> ApplySetAsync(IDSCSet set, IProgress<IDSCSetChangeData> progress = null, CancellationToken ct = default) => _dscOperations.ApplySetAsync(set, progress, ct);

    /// <inheritdoc/>
    public Task<IDSCTestSetResult> TestSetAsync(IDSCSet inputSet, IProgress<IDSCTestUnitResult> progress = null, CancellationToken ct = default) => _dscOperations.TestSetAsync(inputSet, progress, ct);

    /// <inheritdoc/>
    public void GetConfigurationUnitDetails(IDSCSet set) => _dscOperations.GetConfigurationUnitDetails(set);

    /// <inheritdoc/>
    public async Task<IDSCGetUnitResult> GetUnitAsync(IDSCUnit unit, CancellationToken ct) => await _dscOperations.GetUnitAsync(unit, ct);

    /// <inheritdoc/>
    public async Task<IDSCApplyUnitResult> SetUnitAsync(IDSCUnit unit, CancellationToken ct) => await _dscOperations.SetUnitAsync(unit, ct);

    /// <inheritdoc/>
    public async Task<IDSCTestUnitResult> TestUnitAsync(IDSCUnit unit, CancellationToken ct) => await _dscOperations.TestUnitAsync(unit, ct);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ResourceMetada>> GetDscV3ResourcesAsync() => await _dscOperations.GetDscV3ResourcesAsync();
}

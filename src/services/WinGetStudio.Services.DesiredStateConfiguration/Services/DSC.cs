// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;
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
    public Task<IDSCSet> OpenConfigurationSetAsync(IDSCFile file) => _dscOperations.OpenConfigurationSetAsync(file);

    /// <inheritdoc/>
    public IAsyncOperationWithProgress<IDSCApplySetResult, IDSCSetChangeData> ValidateSetAsync(IDSCSet set) => _dscOperations.ValidateSetAsync(set);

    /// <inheritdoc/>
    public IAsyncOperationWithProgress<IDSCApplySetResult, IDSCSetChangeData> ApplySetAsync(IDSCSet set) => _dscOperations.ApplySetAsync(set);

    /// <inheritdoc/>
    public IAsyncOperationWithProgress<IDSCTestSetResult, IDSCTestUnitResult> TestSetAsync(IDSCSet inputSet) => _dscOperations.TestSetAsync(inputSet);

    /// <inheritdoc/>
    public void GetConfigurationUnitDetails(IDSCSet set) => _dscOperations.GetConfigurationUnitDetails(set);

    /// <inheritdoc/>
    public async Task<IDSCGetUnitResult> GetUnitAsync(IDSCUnit unit) => await _dscOperations.GetUnitAsync(unit);

    /// <inheritdoc/>
    public async Task<IDSCApplyUnitResult> SetUnitAsync(IDSCUnit unit) => await _dscOperations.SetUnitAsync(unit);

    /// <inheritdoc/>
    public async Task<IDSCTestUnitResult> TestUnitAsync(IDSCUnit unit) => await _dscOperations.TestUnitAsync(unit);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ResourceMetada>> GetDscV3ResourcesAsync() => await _dscOperations.GetDscV3ResourcesAsync();
}

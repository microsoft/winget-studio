// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;
using WinGetStudio.Services.DesiredStateConfiguration.Models;

namespace WinGetStudio.Services.DesiredStateConfiguration.Contracts;

public interface IDSC
{
    /// <inheritdoc cref="IDSCDeployment.IsUnstubbedAsync" />
    public Task<bool> IsUnstubbedAsync();

    /// <inheritdoc cref="IDSCDeployment.UnstubAsync" />
    public Task<bool> UnstubAsync();

    /// <inheritdoc cref="IDSCOperations.OpenConfigurationSetAsync" />
    public Task<IDSCSet> OpenConfigurationSetAsync(IDSCFile file);

    /// <inheritdoc cref="IDSCOperations.ValidateSetAsync" />/>
    public IAsyncOperationWithProgress<IDSCApplySetResult, IDSCSetChangeData> ValidateSetAsync(IDSCSet dscSet);

    /// <inheritdoc cref="IDSCOperations.ApplySetAsync" />
    public IAsyncOperationWithProgress<IDSCApplySetResult, IDSCSetChangeData> ApplySetAsync(IDSCSet dscSet);

    /// <inheritdoc cref="IDSCOperations.TestSetAsync" />
    public IAsyncOperationWithProgress<IDSCTestSetResult, IDSCTestUnitResult> TestSetAsync(IDSCSet inputSet);

    /// <inheritdoc cref="IDSCOperations.GetConfigurationUnitDetails" />
    public void GetConfigurationUnitDetails(IDSCSet dscSet);

    /// <inheritdoc cref="IDSCOperations.GetUnitAsync" />
    public Task<IDSCGetUnitResult> GetUnitAsync(IDSCUnit unit);

    /// <inheritdoc cref="IDSCOperations.SetUnitAsync"/>
    public Task<IDSCApplyUnitResult> SetUnitAsync(IDSCUnit unit);

    /// <inheritdoc cref="IDSCOperations.TestUnitAsync"/>
    public Task<IDSCTestUnitResult> TestUnitAsync(IDSCUnit unit);

    /// <inheritdoc cref="IDSCOperations.GetDscV3ResourcesAsync"/>
    public Task<IReadOnlyList<ResourceMetada>> GetDscV3ResourcesAsync();
}

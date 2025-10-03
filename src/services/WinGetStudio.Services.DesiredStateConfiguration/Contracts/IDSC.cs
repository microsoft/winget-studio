// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;
using WinGetStudio.Models;
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

    /// <inheritdoc cref="IDSCOperations.ApplySetAsync" />
    public IAsyncOperationWithProgress<IDSCApplySetResult, IDSCSetChangeData> ApplySetAsync(IDSCSet dscSet);

    /// <inheritdoc cref="IDSCOperations.GetConfigurationUnitDetails" />
    public void GetConfigurationUnitDetails(IDSCSet dscSet);

    /// <inheritdoc cref="IDSCOperations.GetUnitAsync" />
    public Task<IDSCGetUnitResult> GetUnitAsync(ConfigurationUnitModel unit);

    /// <inheritdoc cref="IDSCOperations.SetUnitAsync"/>
    public Task SetUnitAsync(ConfigurationUnitModel unit);

    /// <inheritdoc cref="IDSCOperations.TestUnitAsync"/>
    public Task TestUnitAsync(ConfigurationUnitModel unit);

    /// <inheritdoc cref="IDSCOperations.ExportUnitAsync"/>
    public Task ExportUnitAsync(ConfigurationUnitModel unit);

    /// <inheritdoc cref="IDSCOperations.GetDscV3ResourcesAsync"/>
    public Task<IReadOnlyList<ResourceMetada>> GetDscV3ResourcesAsync();
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Windows.Foundation;
using WinGetStudio.Models;

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

    /// <inheritdoc cref="IDSCOperations.Get" />
    public Task DscGet(ConfigurationUnitModel unit);

    /// <inheritdoc cref="IDSCOperations.Set"/>
    public Task DscSet(ConfigurationUnitModel unit);

    /// <inheritdoc cref="IDSCOperations.Test"/>
    public Task DscTest(ConfigurationUnitModel unit);

    /// <inheritdoc cref="IDSCOperations.Export"/>
    public Task DscExport(ConfigurationUnitModel unit);
}

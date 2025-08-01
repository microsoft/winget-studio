// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using WinGetStudio.Models;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using Windows.Foundation;

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
    public IAsyncOperationWithProgress<IDSCApplySetResult, IDSCSetChangeData> ApplySetAsync(IDSCSet set) => _dscOperations.ApplySetAsync(set);

    /// <inheritdoc/>
    public void GetConfigurationUnitDetails(IDSCSet set) => _dscOperations.GetConfigurationUnitDetails(set);

    /// <inheritdoc/>
    public async Task Get(ConfigurationUnitModel unit) => await _dscOperations.GetUnit(unit);

    /// <inheritdoc/>
    public async Task Set(ConfigurationUnitModel unit) => await _dscOperations.SetUnit(unit);

    /// <inheritdoc/>
    public async Task Test(ConfigurationUnitModel unit) => await _dscOperations.TestUnit(unit);

    /// <inheritdoc/>
    public async Task Export(ConfigurationUnitModel unit) => await _dscOperations.ExportUnit(unit);
}

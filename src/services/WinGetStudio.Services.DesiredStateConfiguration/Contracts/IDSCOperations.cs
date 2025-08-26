// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Windows.Foundation;
using WinGetStudio.Models;

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
    /// Apply a DSC configuration set
    /// </summary>
    /// <param name="set">Configuration set to apply</param>
    /// <returns>Result of applying the configuration</returns>
    public IAsyncOperationWithProgress<IDSCApplySetResult, IDSCSetChangeData> ApplySetAsync(IDSCSet set);

    /// <summary>
    /// Get details of configuration units in a set
    /// </summary>
    /// <param name="set">Configuration set to get details for</param>
    public void GetConfigurationUnitDetails(IDSCSet set);

    /// <summary>
    /// Get details for the specified configuration unit.
    /// </summary>
    /// <param name="unit">Unit to get details for</param>
    /// <returns>result of Get</returns>
    public Task GetUnit(ConfigurationUnitModel unit);

    /// <summary>
    /// Set the machine state to the specified configuration unit.
    /// </summary>
    /// <param name="unit">Unit to set</param>
    /// <returns></returns>
    public Task SetUnit(ConfigurationUnitModel unit);

    /// <summary>
    /// Test whether the current machine state is the same as the configuration unit.
    /// </summary>
    /// <param name="unit">Unit to test</param>
    /// <returns>Whether the machine state matches the configuration unit.</returns>
    public Task TestUnit(ConfigurationUnitModel unit);

    /// <summary>
    /// Exports the specified configuration unit.
    /// </summary>
    /// <param name="unit">Unit to export</param>
    /// <returns>All settings for specified unit</returns>
    public Task ExportUnit(ConfigurationUnitModel unit);
}

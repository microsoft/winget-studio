// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WinGetStudio.Services.DesiredStateConfiguration.Models;

namespace WinGetStudio.Services.DesiredStateConfiguration.Contracts;

public interface IDSC
{
    /// <inheritdoc cref="IDSCDeployment.IsUnstubbedAsync" />
    Task<bool> IsUnstubbedAsync();

    /// <inheritdoc cref="IDSCDeployment.UnstubAsync" />
    Task<bool> UnstubAsync();

    /// <inheritdoc cref="IDSCOperations.OpenConfigurationSetAsync" />
    Task<IDSCSet> OpenConfigurationSetAsync(IDSCFile file, CancellationToken ct = default);

    /// <inheritdoc cref="IDSCOperations.ValidateSetAsync" />/>
    Task<IDSCApplySetResult> ValidateSetAsync(IDSCSet dscSet, IProgress<IDSCSetChangeData> progress = null, CancellationToken ct = default);

    /// <inheritdoc cref="IDSCOperations.ApplySetAsync" />
    Task<IDSCApplySetResult> ApplySetAsync(IDSCSet dscSet, IProgress<IDSCSetChangeData> progress = null, CancellationToken ct = default);

    /// <inheritdoc cref="IDSCOperations.TestSetAsync" />
    Task<IDSCTestSetResult> TestSetAsync(IDSCSet inputSet, IProgress<IDSCTestUnitResult> progress = null,  CancellationToken ct = default);

    /// <inheritdoc cref="IDSCOperations.GetConfigurationUnitDetails" />
    void GetConfigurationUnitDetails(IDSCSet dscSet);

    /// <inheritdoc cref="IDSCOperations.GetUnitAsync" />
    Task<IDSCGetUnitResult> GetUnitAsync(IDSCUnit unit, CancellationToken ct = default);

    /// <inheritdoc cref="IDSCOperations.SetUnitAsync"/>
    Task<IDSCApplyUnitResult> SetUnitAsync(IDSCUnit unit, CancellationToken ct = default);

    /// <inheritdoc cref="IDSCOperations.TestUnitAsync"/>
    Task<IDSCTestUnitResult> TestUnitAsync(IDSCUnit unit, CancellationToken ct = default);

    /// <inheritdoc cref="IDSCOperations.GetDscV3ResourcesAsync"/>
    Task<IReadOnlyList<ResourceMetada>> GetDscV3ResourcesAsync();
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Management.Configuration;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;

namespace WinGetStudio.Services.DesiredStateConfiguration.Models;

internal sealed class DSCTestUnitResult : IDSCTestUnitResult
{
    /// <inheritdoc/>
    public IDSCUnit Unit { get; }

    /// <inheritdoc/>
    public IDSCUnitResultInformation ResultInformation { get; }

    /// <inheritdoc/>
    public ConfigurationTestResult TestResult { get; }

    public DSCTestUnitResult(TestConfigurationUnitResult unitResult)
    {
        // Constructor copies all the required data from the out-of-proc COM
        // objects over to the current process. This ensures that we have this
        // information available even if the out-of-proc COM objects are no
        // longer available (e.g. AppInstaller service is no longer running).
        Unit = new DSCUnit(unitResult.Unit);
        TestResult = unitResult.TestResult;
        ResultInformation = unitResult.ResultInformation == null ? null : new DSCUnitResultInformation(unitResult.ResultInformation);
    }
}

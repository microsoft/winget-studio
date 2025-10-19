// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Management.Configuration;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;

namespace WinGetStudio.Services.DesiredStateConfiguration.Models;

internal sealed class DSCTestSetResult : IDSCTestSetResult
{
    public IReadOnlyList<IDSCTestUnitResult> UnitResults { get; }

    public ConfigurationTestResult TestResult { get; }

    public DSCTestSetResult(TestConfigurationSetResult result)
    {
        // Constructor copies all the required data from the out-of-proc COM
        // objects over to the current process. This ensures that we have this
        // information available even if the out-of-proc COM objects are no
        // longer available (e.g. AppInstaller service is no longer running).
        UnitResults = result.UnitResults.Select(unitResult => new DSCTestUnitResult(unitResult)).ToList();
        TestResult = result.TestResult;
    }
}

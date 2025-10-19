// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Management.Configuration;

namespace WinGetStudio.Services.DesiredStateConfiguration.Contracts;

public interface IDSCTestSetResult
{
    /// <summary>
    /// Gets the results for each unit in the set.
    /// </summary>
    IReadOnlyList<IDSCTestUnitResult> UnitResults { get; }

    /// <summary>
    /// Gets the overall test result for the set.
    /// </summary>
    ConfigurationTestResult TestResult { get; }
}

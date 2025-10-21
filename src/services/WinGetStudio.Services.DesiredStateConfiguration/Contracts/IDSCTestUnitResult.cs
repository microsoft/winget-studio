// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Management.Configuration;

namespace WinGetStudio.Services.DesiredStateConfiguration.Contracts;

public interface IDSCTestUnitResult
{
    /// <summary>
    /// Gets the configuration unit that was tested.
    /// </summary>
    IDSCUnit Unit { get; }

    /// <summary>
    /// Gets the result information for the operation.
    /// </summary>
    IDSCUnitResultInformation ResultInformation { get; }

    /// <summary>
    /// Gets the result of the test operation.
    /// </summary>
    ConfigurationTestResult TestResult { get; }
}

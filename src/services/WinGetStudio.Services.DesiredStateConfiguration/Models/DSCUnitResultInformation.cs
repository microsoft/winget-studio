// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Management.Configuration;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;

namespace WinGetStudio.Services.DesiredStateConfiguration.Models;

internal sealed class DSCUnitResultInformation : IDSCUnitResultInformation
{
    /// <inheritdoc/>
    public Exception ResultCode { get; }

    /// <inheritdoc/>
    public string Description { get; }

    /// <inheritdoc/>
    public string Details { get; }

    /// <inheritdoc/>
    public ConfigurationUnitResultSource ResultSource { get; }

    public DSCUnitResultInformation(IConfigurationUnitResultInformation resultInformation)
    {
        // Constructor copies all the required data from the out-of-proc COM
        // objects over to the current process. This ensures that we have this
        // information available even if the out-of-proc COM objects are no
        // longer available (e.g. AppInstaller service is no longer running).
        ResultCode = resultInformation.ResultCode;
        Description = resultInformation.Description;
        Details = resultInformation.Details;
        ResultSource = resultInformation.ResultSource;
    }
}

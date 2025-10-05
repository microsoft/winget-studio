// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Management.Configuration;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Extensions;

namespace WinGetStudio.Services.DesiredStateConfiguration.Models;

internal sealed class DSCGetUnitResult : IDSCGetUnitResult
{
    /// <inheritdoc/>
    public IDSCUnitResultInformation ResultInformation { get; }

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, object> Settings { get; }

    public DSCGetUnitResult(GetConfigurationUnitSettingsResult settingsResult)
    {
        // Constructor copies all the required data from the out-of-proc COM
        // objects over to the current process. This ensures that we have this
        // information available even if the out-of-proc COM objects are no
        // longer available (e.g. AppInstaller service is no longer running).
        ResultInformation = new DSCUnitResultInformation(settingsResult.ResultInformation);
        Settings = settingsResult.Settings == null ? [] : settingsResult.Settings.DeepCopyViaYaml();
    }
}

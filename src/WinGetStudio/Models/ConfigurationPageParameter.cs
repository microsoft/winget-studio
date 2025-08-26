// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.Services.DesiredStateConfiguration.Contracts;

namespace WinGetStudio.Models;

public class ConfigurationPageParameter
{
    public IDSCSet? DSCSet { get; set; }

    public bool ResetDSCSet { get; set; }
}

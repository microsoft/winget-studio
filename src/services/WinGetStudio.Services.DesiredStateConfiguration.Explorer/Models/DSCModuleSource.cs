// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

public enum DSCModuleSource
{
    /// <summary>
    /// Represents the PowerShell Gallery as the source of the module.
    /// </summary>
    PSGallery,

    /// <summary>
    /// Represents the local DSC v3 resource as the source of the module.
    /// </summary>
    LocalDscV3,
}

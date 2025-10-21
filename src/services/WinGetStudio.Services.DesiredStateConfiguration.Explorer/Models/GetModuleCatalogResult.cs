// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

public sealed partial class GetModuleCatalogResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the result can be cached.
    /// </summary>
    public bool CanCache { get; set; }

    /// <summary>
    /// Gets or sets the catalog of DSC modules.
    /// </summary>
    public DSCModuleCatalog Catalog { get; set; }
}

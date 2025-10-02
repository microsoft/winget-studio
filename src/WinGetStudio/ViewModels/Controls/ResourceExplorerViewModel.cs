// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

namespace WinGetStudio.ViewModels.Controls;

public sealed partial class ResourceExplorerViewModel : ObservableRecipient
{
    /// <summary>
    /// Gets or sets the properties of the resource.
    /// </summary>
    [ObservableProperty]
    public partial List<DSCProperty> Properties { get; set; }

    /// <summary>
    /// Gets or sets the syntax of the resource.
    /// </summary>
    [ObservableProperty]
    public partial string ResourceSyntax { get; set; }

    /// <summary>
    /// Sets the resource to display.
    /// </summary>
    /// <param name="resource">The DSC resource.</param>
    public void SetResource(DSCResource resource)
    {
        Properties = [..resource.Properties];
        ResourceSyntax = resource.Syntax;
    }
}

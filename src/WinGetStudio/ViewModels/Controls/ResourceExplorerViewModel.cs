// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

namespace WinGetStudio.ViewModels.Controls;

public sealed partial class ResourceExplorerViewModel : ObservableRecipient
{
    [ObservableProperty]
    public partial List<DSCProperty> Properties { get; set; }

    [ObservableProperty]
    public partial string ResourceSyntax { get; set; }

    public void SetResource(DSCResource resource)
    {
        Properties = [..resource.Properties];
        ResourceSyntax = resource.Syntax;
    }
}

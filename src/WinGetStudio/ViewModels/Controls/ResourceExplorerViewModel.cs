// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

namespace WinGetStudio.ViewModels.Controls;

public sealed partial class ResourceExplorerViewModel : ObservableRecipient
{
    private readonly IDSCExplorer _dscExplorer;
    private DSCResource? _resource;

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

    public ResourceExplorerViewModel(IDSCExplorer explorer)
    {
        _dscExplorer = explorer;
        Properties = [];
        ResourceSyntax = string.Empty;
    }

    /// <summary>
    /// Sets the resource to display.
    /// </summary>
    /// <param name="resource">The DSC resource.</param>
    public void SetResource(DSCResource resource)
    {
        _resource = resource;
        Properties = [..resource.Properties];
        ResourceSyntax = resource.Syntax;
    }

    public async Task<string?> GetSampleYamlAsync()
    {
        if (_resource != null)
        {
            return await _dscExplorer.GetSampleYamlAsync(_resource);
        }

        return null;
    }
}

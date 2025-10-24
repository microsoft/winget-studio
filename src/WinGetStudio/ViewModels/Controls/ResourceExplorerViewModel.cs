// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

namespace WinGetStudio.ViewModels.Controls;

public delegate ResourceExplorerViewModel ResourceExplorerViewModelFactory(DSCResource resource);

public sealed partial class ResourceExplorerViewModel : ObservableRecipient
{
    private readonly IDSCExplorer _dscExplorer;
    private readonly DSCResource _resource;

    /// <summary>
    /// Gets the properties of the resource.
    /// </summary>
    public List<DSCProperty> Properties => _resource.Properties;

    /// <summary>
    /// Gets the resource code.
    /// </summary>
    public string ResourceCode => _resource.Code;

    /// <summary>
    /// Gets the resource syntax.
    /// </summary>
    public string ResourceSyntax => _resource.Syntax;

    public ResourceExplorerViewModel(DSCResource resource, IDSCExplorer explorer)
    {
        _resource = resource;
        _dscExplorer = explorer;
    }

    /// <summary>
    /// Gets a sample YAML for the resource.
    /// </summary>
    /// <returns>The sample YAML.</returns>
    public async Task<string?> GenerateDefaultYamlAsync()
    {
        if (_resource != null)
        {
            return await _dscExplorer.GenerateDefaultYamlAsync(_resource);
        }

        return null;
    }
}

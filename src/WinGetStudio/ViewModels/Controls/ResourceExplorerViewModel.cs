// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

namespace WinGetStudio.ViewModels.Controls;

public sealed partial class ResourceExplorerViewModel : ObservableRecipient
{
    private readonly IDSCExplorer _dscExplorer;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Properties))]
    [NotifyPropertyChangedFor(nameof(Code))]
    [NotifyPropertyChangedFor(nameof(Syntax))]
    [NotifyPropertyChangedFor(nameof(CanGenerateYaml))]
    [NotifyPropertyChangedFor(nameof(CanShowCode))]
    [NotifyPropertyChangedFor(nameof(CanShowJsonSchema))]
    [NotifyPropertyChangedFor(nameof(HasProperties))]
    [NotifyCanExecuteChangedFor(nameof(ShowJsonSchemaCommand))]
    public partial DSCResource? Resource { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSummaryView))]
    public partial bool IsCodeView { get; set; }

    /// <summary>
    /// Gets a value indicating whether the summary view is active.
    /// </summary>
    public bool IsSummaryView => !IsCodeView;

    /// <summary>
    /// Gets a value indicating whether YAML can be generated.
    /// </summary>
    public bool CanGenerateYaml => Resource != null;

    /// <summary>
    /// Gets a value indicating whether the code can be displayed.
    /// </summary>
    public bool CanShowCode => !string.IsNullOrWhiteSpace(Resource?.Code);

    /// <summary>
    /// Gets a value indicating whether the schema can be viewed.
    /// </summary>
    public bool CanShowJsonSchema => Resource?.DSCVersion == DSCVersion.V3;

    /// <summary>
    /// Gets a value indicating whether the resource has properties.
    /// </summary>
    public bool HasProperties => Resource?.Properties?.Count > 0;

    /// <summary>
    /// Gets the properties of the resource.
    /// </summary>
    public List<DSCProperty>? Properties => Resource?.Properties;

    /// <summary>
    /// Gets the resource code.
    /// </summary>
    public string? Code => Resource?.Code;

    /// <summary>
    /// Gets the syntax of the resource.
    /// </summary>
    public string Syntax => Resource?.DSCVersion == DSCVersion.V3 ? "json" : "powershell";

    public ResourceExplorerViewModel(IDSCExplorer explorer)
    {
        _dscExplorer = explorer;
    }

    /// <summary>
    /// Gets a sample YAML for the resource.
    /// </summary>
    /// <returns>The sample YAML.</returns>
    public async Task<string?> GenerateDefaultYamlAsync()
    {
        if (Resource != null)
        {
            return await _dscExplorer.GenerateDefaultYamlAsync(Resource);
        }

        return null;
    }

    [RelayCommand(CanExecute = nameof(CanShowJsonSchema))]
    private void OnShowJsonSchema()
    {
        IsCodeView = true;
    }

    partial void OnResourceChanged(DSCResource? oldValue, DSCResource? newValue)
    {
        // Always switch back to summary view when the resource changes.
        IsCodeView = false;
    }
}

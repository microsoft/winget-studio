// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using WinGetStudio.ViewModels;

namespace WinGetStudio.Models.Graph;

/// <summary>
/// Represents a node in the DAG visualization, positioned by the layout engine.
/// </summary>
public partial class DagNode : ObservableObject
{
    public DagNode(UnitViewModel unit, string id)
    {
        Unit = unit;
        Id = id;
    }

    /// <summary>
    /// Gets the unique identifier for this node (matches the unit's IdOrDefault).
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the source UnitViewModel this node represents.
    /// </summary>
    public UnitViewModel Unit { get; }

    /// <summary>
    /// Gets or sets the display label (resource type / name).
    /// </summary>
    [ObservableProperty]
    public partial string Label { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the X coordinate computed by the layout engine.
    /// </summary>
    [ObservableProperty]
    public partial double X { get; set; }

    /// <summary>
    /// Gets or sets the Y coordinate computed by the layout engine.
    /// </summary>
    [ObservableProperty]
    public partial double Y { get; set; }

    /// <summary>
    /// Gets or sets the width of this node.
    /// </summary>
    [ObservableProperty]
    public partial double Width { get; set; } = 200;

    /// <summary>
    /// Gets or sets the height of this node.
    /// </summary>
    [ObservableProperty]
    public partial double Height { get; set; } = 60;

    /// <summary>
    /// Gets or sets whether this node is currently selected.
    /// </summary>
    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    /// <summary>
    /// Gets or sets whether this node is highlighted (as a dependency of the selected node).
    /// </summary>
    [ObservableProperty]
    public partial bool IsHighlighted { get; set; }
}

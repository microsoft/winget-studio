// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;

namespace WinGetStudio.Models.Graph;

/// <summary>
/// Represents a directed edge in the DAG visualization.
/// The edge goes from the dependency node (Source) to the dependent node (Target),
/// indicating order of operations: Source must complete before Target.
/// </summary>
public partial class DagEdge : ObservableObject
{
    public DagEdge(DagNode source, DagNode target)
    {
        Source = source;
        Target = target;
    }

    /// <summary>
    /// Gets the source node (the dependency — runs first).
    /// </summary>
    public DagNode Source { get; }

    /// <summary>
    /// Gets the target node (the dependent — runs after source).
    /// </summary>
    public DagNode Target { get; }

    /// <summary>
    /// Gets or sets the SVG-style path data string for rendering the edge.
    /// </summary>
    [ObservableProperty]
    public partial string PathData { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the arrowhead path data string.
    /// </summary>
    [ObservableProperty]
    public partial string ArrowHeadData { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this edge is highlighted.
    /// </summary>
    [ObservableProperty]
    public partial bool IsHighlighted { get; set; }
}

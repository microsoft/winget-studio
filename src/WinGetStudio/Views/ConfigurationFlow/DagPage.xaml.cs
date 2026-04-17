// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Specialized;
using System.ComponentModel;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WinGetStudio.Contracts.Views;
using WinGetStudio.Models.Graph;
using WinGetStudio.ViewModels.ConfigurationFlow;

using Path = Microsoft.UI.Xaml.Shapes.Path;

namespace WinGetStudio.Views.ConfigurationFlow;

/// <summary>
/// Page that renders the DAG dependency graph using a Canvas.
/// Nodes are rendered as <see cref="DagNodeControl"/> elements and edges as Path elements.
/// </summary>
public sealed partial class DagPage : Page, IView<DagViewModel>
{
    private static readonly SolidColorBrush DefaultEdgeBrush = new(Colors.Gray);
    private static readonly SolidColorBrush HighlightedEdgeBrush = new(Colors.Orange);

    private readonly Dictionary<DagEdge, PropertyChangedEventHandler> _edgeHandlers = [];

    public DagViewModel ViewModel { get; }

    public DagPage()
    {
        ViewModel = App.GetService<DagViewModel>();
        InitializeComponent();

        ViewModel.Nodes.CollectionChanged += OnNodesCollectionChanged;
        ViewModel.Edges.CollectionChanged += OnEdgesCollectionChanged;

        Unloaded += OnUnloaded;
    }

    /// <summary>
    /// Cleans up event subscriptions when the page is unloaded.
    /// </summary>
    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.Nodes.CollectionChanged -= OnNodesCollectionChanged;
        ViewModel.Edges.CollectionChanged -= OnEdgesCollectionChanged;
        ClearCanvasNodes();
        ClearCanvasEdges();
    }

    private void OnNodesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            ClearCanvasNodes();
        }

        if (e.NewItems != null)
        {
            foreach (DagNode node in e.NewItems)
            {
                AddNodeToCanvas(node);
            }
        }
    }

    private void OnEdgesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            ClearCanvasEdges();
        }

        if (e.NewItems != null)
        {
            foreach (DagEdge edge in e.NewItems)
            {
                AddEdgeToCanvas(edge);
            }
        }
    }

    /// <summary>
    /// Creates a <see cref="DagNodeControl"/> for the given node and positions it on the canvas.
    /// </summary>
    private void AddNodeToCanvas(DagNode node)
    {
        var control = new DagNodeControl { Node = node };
        control.NodeTapped += OnDagNodeTapped;

        Canvas.SetLeft(control, node.X);
        Canvas.SetTop(control, node.Y);
        Canvas.SetZIndex(control, 1);

        DagCanvas.Children.Add(control);
    }

    /// <summary>
    /// Creates Path elements for the edge line and arrowhead, and subscribes to highlight changes.
    /// </summary>
    private void AddEdgeToCanvas(DagEdge edge)
    {
        if (string.IsNullOrEmpty(edge.PathData))
        {
            return;
        }

        // Edge line
        var path = new Path
        {
            Data = (Geometry)Microsoft.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(typeof(Geometry), edge.PathData),
            Stroke = DefaultEdgeBrush,
            StrokeThickness = 2,
            Tag = edge,
        };

        Canvas.SetZIndex(path, 0);
        DagCanvas.Children.Add(path);

        // Arrowhead
        if (!string.IsNullOrEmpty(edge.ArrowHeadData))
        {
            var arrowPath = new Path
            {
                Data = (Geometry)Microsoft.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(typeof(Geometry), edge.ArrowHeadData),
                Fill = DefaultEdgeBrush,
                Tag = edge,
            };

            Canvas.SetZIndex(arrowPath, 0);
            DagCanvas.Children.Add(arrowPath);
        }

        // Subscribe to highlight changes with trackable handler.
        PropertyChangedEventHandler handler = (s, e) =>
        {
            if (e.PropertyName == nameof(DagEdge.IsHighlighted))
            {
                DispatcherQueue.TryEnqueue(() => UpdateEdgeHighlight(edge));
            }
        };

        edge.PropertyChanged += handler;
        _edgeHandlers[edge] = handler;
    }

    /// <summary>
    /// Updates the stroke and fill of all Path elements associated with the given edge.
    /// </summary>
    private void UpdateEdgeHighlight(DagEdge edge)
    {
        var brush = edge.IsHighlighted ? HighlightedEdgeBrush : DefaultEdgeBrush;
        var thickness = edge.IsHighlighted ? 3.0 : 2.0;

        foreach (var child in DagCanvas.Children)
        {
            if (child is Path path && path.Tag == edge)
            {
                path.Stroke = brush;
                path.StrokeThickness = thickness;
                if (path.Fill != null)
                {
                    path.Fill = brush;
                }
            }
        }
    }

    /// <summary>
    /// Removes all node controls from the canvas and unsubscribes their event handlers.
    /// </summary>
    private void ClearCanvasNodes()
    {
        var toRemove = DagCanvas.Children.OfType<DagNodeControl>().ToList();
        foreach (var control in toRemove)
        {
            control.NodeTapped -= OnDagNodeTapped;
            control.Node = null!;
            DagCanvas.Children.Remove(control);
        }
    }

    /// <summary>
    /// Removes all edge Path elements from the canvas and unsubscribes PropertyChanged handlers.
    /// </summary>
    private void ClearCanvasEdges()
    {
        foreach (var (edge, handler) in _edgeHandlers)
        {
            edge.PropertyChanged -= handler;
        }

        _edgeHandlers.Clear();

        var toRemove = DagCanvas.Children.OfType<Path>().ToList();
        foreach (var path in toRemove)
        {
            DagCanvas.Children.Remove(path);
        }
    }

    private void OnDagNodeTapped(object? sender, DagNode node)
    {
        ViewModel.OnNodeSelected(node);
    }
}

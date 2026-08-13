// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Core.Routing;
using Microsoft.Msagl.Layout.Layered;
using WinGetStudio.ViewModels;

namespace WinGetStudio.Models.Graph;

/// <summary>
/// Translates UnitViewModels into a positioned DAG layout using MSAGL's Sugiyama algorithm.
/// </summary>
public static class DagLayoutService
{
    private const double DefaultNodeWidth = 200;
    private const double DefaultNodeHeight = 60;
    private const double ArrowHeadSize = 8;

    /// <summary>
    /// Computes the DAG layout for the given set of units and their dependencies.
    /// </summary>
    /// <param name="units">The configuration units to lay out.</param>
    /// <returns>The computed nodes and edges with positions.</returns>
    /// <exception cref="InvalidOperationException">Thrown when a cycle is detected.</exception>
    public static DagLayoutResult ComputeLayout(IReadOnlyList<UnitViewModel> units)
    {
        if (units == null || units.Count == 0)
        {
            return new DagLayoutResult([], []);
        }

        var graph = new GeometryGraph();
        var nodeMap = new Dictionary<string, (Microsoft.Msagl.Core.Layout.Node MsaglNode, DagNode DagNode)>();

        // Create MSAGL nodes for each unit.
        foreach (var unit in units)
        {
            var id = unit.IdOrDefault;
            var label = unit.Title ?? id;

            var dagNode = new DagNode(unit, id) { Label = label };
            var msaglNode = new Microsoft.Msagl.Core.Layout.Node(
                CurveFactory.CreateRectangle(DefaultNodeWidth, DefaultNodeHeight, new Point(0, 0)),
                id);

            graph.Nodes.Add(msaglNode);
            nodeMap[id] = (msaglNode, dagNode);
        }

        // Create edges for dependencies.
        // Edge direction: dependency → dependent (arrow shows order of operations).
        var edgePairs = new List<(Microsoft.Msagl.Core.Layout.Edge MsaglEdge, DagEdge DagEdge)>();
        foreach (var unit in units)
        {
            if (unit.Dependencies == null)
            {
                continue;
            }

            var dependentId = unit.IdOrDefault;
            foreach (var dep in unit.Dependencies)
            {
                var dependencyId = dep.IdOrDefault;
                if (nodeMap.TryGetValue(dependencyId, out var dependency) && nodeMap.TryGetValue(dependentId, out var dependent))
                {
                    var msaglEdge = new Microsoft.Msagl.Core.Layout.Edge(dependency.MsaglNode, dependent.MsaglNode)
                    {
                        EdgeGeometry = new EdgeGeometry { TargetArrowhead = new Arrowhead() },
                    };
                    graph.Edges.Add(msaglEdge);

                    var dagEdge = new DagEdge(dependency.DagNode, dependent.DagNode);
                    edgePairs.Add((msaglEdge, dagEdge));
                }
            }
        }

        // Run MSAGL Sugiyama layout.
        var settings = new SugiyamaLayoutSettings
        {
            EdgeRoutingSettings = { EdgeRoutingMode = EdgeRoutingMode.Spline },
        };

        var layout = new LayeredLayout(graph, settings);
        layout.Run();

        // Read back positions from MSAGL and normalize so the top-left is near (0,0).
        // MSAGL uses Y-up coordinates; WinUI Canvas uses Y-down, so we flip the Y axis.
        var minX = graph.Nodes.Min(n => n.BoundingBox.Left);
        var maxY = graph.Nodes.Max(n => n.BoundingBox.Top);
        var padding = 40.0;

        var dagNodes = new List<DagNode>();
        foreach (var entry in nodeMap.Values)
        {
            var msaglNode = entry.MsaglNode;
            var dagNode = entry.DagNode;

            dagNode.X = msaglNode.BoundingBox.Left - minX + padding;
            dagNode.Y = maxY - msaglNode.BoundingBox.Top + padding;
            dagNode.Width = msaglNode.BoundingBox.Width;
            dagNode.Height = msaglNode.BoundingBox.Height;

            dagNodes.Add(dagNode);
        }

        // Build edge path data from MSAGL spline geometry.
        var dagEdges = new List<DagEdge>();
        foreach (var (msaglEdge, dagEdge) in edgePairs)
        {
            dagEdge.PathData = BuildEdgePathData(msaglEdge, minX, maxY, padding);
            dagEdge.ArrowHeadData = BuildArrowHeadData(msaglEdge, minX, maxY, padding);
            dagEdges.Add(dagEdge);
        }

        return new DagLayoutResult(dagNodes, dagEdges);
    }

    /// <summary>
    /// Converts an MSAGL edge curve into a XAML path data string.
    /// </summary>
    private static string BuildEdgePathData(Microsoft.Msagl.Core.Layout.Edge edge, double minX, double maxY, double padding)
    {
        var curve = edge.Curve;
        if (curve == null)
        {
            return string.Empty;
        }

        return curve switch
        {
            Curve compositeCurve => BuildCompositeCurvePathData(compositeCurve, minX, maxY, padding),
            LineSegment line => BuildLinePathData(line, minX, maxY, padding),
            CubicBezierSegment bezier => BuildBezierPathData(bezier, minX, maxY, padding),
            _ => string.Empty,
        };
    }

    /// <summary>
    /// Converts a composite MSAGL curve (multiple segments) into a XAML path data string.
    /// </summary>
    private static string BuildCompositeCurvePathData(Curve curve, double minX, double maxY, double padding)
    {
        var segments = curve.Segments.ToList();
        if (segments.Count == 0)
        {
            return string.Empty;
        }

        var path = new System.Text.StringBuilder();
        var first = segments[0];
        var startPoint = TranslatePoint(first.Start, minX, maxY, padding);
        path.Append(CultureInfo.InvariantCulture, $"M {startPoint.X:F1} {startPoint.Y:F1}");

        foreach (var segment in segments)
        {
            if (segment is CubicBezierSegment bezier)
            {
                var b1 = TranslatePoint(bezier.B(1), minX, maxY, padding);
                var b2 = TranslatePoint(bezier.B(2), minX, maxY, padding);
                var b3 = TranslatePoint(bezier.B(3), minX, maxY, padding);
                path.Append(CultureInfo.InvariantCulture, $" C {b1.X:F1} {b1.Y:F1} {b2.X:F1} {b2.Y:F1} {b3.X:F1} {b3.Y:F1}");
            }
            else if (segment is LineSegment line)
            {
                var end = TranslatePoint(line.End, minX, maxY, padding);
                path.Append(CultureInfo.InvariantCulture, $" L {end.X:F1} {end.Y:F1}");
            }
        }

        return path.ToString();
    }

    /// <summary>
    /// Converts an MSAGL line segment into a XAML path data string.
    /// </summary>
    private static string BuildLinePathData(LineSegment line, double minX, double maxY, double padding)
    {
        var start = TranslatePoint(line.Start, minX, maxY, padding);
        var end = TranslatePoint(line.End, minX, maxY, padding);
        return string.Create(CultureInfo.InvariantCulture, $"M {start.X:F1} {start.Y:F1} L {end.X:F1} {end.Y:F1}");
    }

    /// <summary>
    /// Converts an MSAGL cubic Bézier segment into a XAML path data string.
    /// </summary>
    private static string BuildBezierPathData(CubicBezierSegment bezier, double minX, double maxY, double padding)
    {
        var b0 = TranslatePoint(bezier.B(0), minX, maxY, padding);
        var b1 = TranslatePoint(bezier.B(1), minX, maxY, padding);
        var b2 = TranslatePoint(bezier.B(2), minX, maxY, padding);
        var b3 = TranslatePoint(bezier.B(3), minX, maxY, padding);
        return string.Create(CultureInfo.InvariantCulture, $"M {b0.X:F1} {b0.Y:F1} C {b1.X:F1} {b1.Y:F1} {b2.X:F1} {b2.Y:F1} {b3.X:F1} {b3.Y:F1}");
    }

    /// <summary>
    /// Builds a triangular arrowhead path data string at the target end of an edge.
    /// </summary>
    private static string BuildArrowHeadData(Microsoft.Msagl.Core.Layout.Edge edge, double minX, double maxY, double padding)
    {
        var arrowhead = edge.EdgeGeometry?.TargetArrowhead;
        if (arrowhead == null)
        {
            return string.Empty;
        }

        var tip = TranslatePoint(arrowhead.TipPosition, minX, maxY, padding);

        // Compute arrowhead direction from the edge curve's end tangent.
        var curve = edge.Curve;
        if (curve == null)
        {
            return string.Empty;
        }

        var msaglDirection = (curve.End - curve[curve.ParEnd - 0.01]).Normalize();

        // Flip Y to match the canvas coordinate system (Y-down).
        var direction = new Point(msaglDirection.X, -msaglDirection.Y);
        var perpendicular = new Point(-direction.Y, direction.X);

        var left = new Point(
            tip.X - (direction.X * ArrowHeadSize) + (perpendicular.X * ArrowHeadSize / 2),
            tip.Y - (direction.Y * ArrowHeadSize) + (perpendicular.Y * ArrowHeadSize / 2));
        var right = new Point(
            tip.X - (direction.X * ArrowHeadSize) - (perpendicular.X * ArrowHeadSize / 2),
            tip.Y - (direction.Y * ArrowHeadSize) - (perpendicular.Y * ArrowHeadSize / 2));

        return string.Create(CultureInfo.InvariantCulture, $"M {tip.X:F1} {tip.Y:F1} L {left.X:F1} {left.Y:F1} L {right.X:F1} {right.Y:F1} Z");
    }

    /// <summary>
    /// Translates an MSAGL point to canvas coordinates by applying the normalization offset,
    /// Y-axis flip (MSAGL Y-up to WinUI Y-down), and padding.
    /// </summary>
    private static Point TranslatePoint(Point point, double minX, double maxY, double padding)
    {
        return new Point(point.X - minX + padding, maxY - point.Y + padding);
    }
}

/// <summary>
/// Contains the result of a DAG layout computation.
/// </summary>
public sealed class DagLayoutResult
{
    public DagLayoutResult(IReadOnlyList<DagNode> nodes, IReadOnlyList<DagEdge> edges)
    {
        Nodes = nodes;
        Edges = edges;
    }

    public IReadOnlyList<DagNode> Nodes { get; }

    public IReadOnlyList<DagEdge> Edges { get; }

    /// <summary>
    /// Gets the total width needed for the canvas.
    /// </summary>
    public double TotalWidth => Nodes.Count > 0 ? Nodes.Max(n => n.X + n.Width) + 40 : 0;

    /// <summary>
    /// Gets the total height needed for the canvas.
    /// </summary>
    public double TotalHeight => Nodes.Count > 0 ? Nodes.Max(n => n.Y + n.Height) + 40 : 0;
}

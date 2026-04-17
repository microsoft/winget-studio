// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.Models.Graph;
using WinGetStudio.ViewModels;

namespace WinGetStudio.Tests.MSTest;

[TestClass]
public class DagLayoutServiceTests
{
    private static UnitViewModel CreateUnit(string id, string? title = null, List<UnitViewModel>? dependencies = null)
    {
        var unit = new UnitViewModel(null!, null!, null!, null!)
        {
            Id = id,
            Title = title ?? id,
            Dependencies = dependencies ?? [],
        };
        return unit;
    }

    [TestMethod]
    public void ComputeLayout_EmptyList_ReturnsEmptyResult()
    {
        var result = DagLayoutService.ComputeLayout([]);

        Assert.AreEqual(0, result.Nodes.Count);
        Assert.AreEqual(0, result.Edges.Count);
        Assert.AreEqual(0, result.TotalWidth);
        Assert.AreEqual(0, result.TotalHeight);
    }

    [TestMethod]
    public void ComputeLayout_NullList_ReturnsEmptyResult()
    {
        var result = DagLayoutService.ComputeLayout(null!);

        Assert.AreEqual(0, result.Nodes.Count);
        Assert.AreEqual(0, result.Edges.Count);
    }

    [TestMethod]
    public void ComputeLayout_SingleNode_ReturnsOneNodeNoEdges()
    {
        var unit = CreateUnit("A");
        var result = DagLayoutService.ComputeLayout([unit]);

        Assert.AreEqual(1, result.Nodes.Count);
        Assert.AreEqual(0, result.Edges.Count);
        Assert.AreEqual("A", result.Nodes[0].Id);
        Assert.AreEqual("A", result.Nodes[0].Label);
        Assert.IsTrue(result.Nodes[0].Width > 0);
        Assert.IsTrue(result.Nodes[0].Height > 0);
    }

    [TestMethod]
    public void ComputeLayout_LinearChain_CreatesCorrectEdges()
    {
        var unitA = CreateUnit("A");
        var unitB = CreateUnit("B", dependencies: [unitA]);
        var unitC = CreateUnit("C", dependencies: [unitB]);

        var result = DagLayoutService.ComputeLayout([unitA, unitB, unitC]);

        Assert.AreEqual(3, result.Nodes.Count);
        Assert.AreEqual(2, result.Edges.Count);

        // Verify edge directions: A→B and B→C (order of operations).
        var edgeAB = result.Edges.FirstOrDefault(e => e.Source.Id == "A" && e.Target.Id == "B");
        var edgeBC = result.Edges.FirstOrDefault(e => e.Source.Id == "B" && e.Target.Id == "C");
        Assert.IsNotNull(edgeAB, "Expected edge from A to B");
        Assert.IsNotNull(edgeBC, "Expected edge from B to C");
    }

    [TestMethod]
    public void ComputeLayout_DiamondPattern_CreatesFourEdges()
    {
        //   A
        //  / \
        // B   C
        //  \ /
        //   D
        var unitA = CreateUnit("A");
        var unitB = CreateUnit("B", dependencies: [unitA]);
        var unitC = CreateUnit("C", dependencies: [unitA]);
        var unitD = CreateUnit("D", dependencies: [unitB, unitC]);

        var result = DagLayoutService.ComputeLayout([unitA, unitB, unitC, unitD]);

        Assert.AreEqual(4, result.Nodes.Count);
        Assert.AreEqual(4, result.Edges.Count);
    }

    [TestMethod]
    public void ComputeLayout_AllNodesGetPositiveCoordinates()
    {
        var unitA = CreateUnit("A");
        var unitB = CreateUnit("B", dependencies: [unitA]);
        var unitC = CreateUnit("C", dependencies: [unitA]);

        var result = DagLayoutService.ComputeLayout([unitA, unitB, unitC]);

        foreach (var node in result.Nodes)
        {
            Assert.IsTrue(node.X >= 0, $"Node {node.Id} has negative X: {node.X}");
            Assert.IsTrue(node.Y >= 0, $"Node {node.Id} has negative Y: {node.Y}");
        }
    }

    [TestMethod]
    public void ComputeLayout_EdgesHavePathData()
    {
        var unitA = CreateUnit("A");
        var unitB = CreateUnit("B", dependencies: [unitA]);

        var result = DagLayoutService.ComputeLayout([unitA, unitB]);

        Assert.AreEqual(1, result.Edges.Count);
        Assert.IsFalse(string.IsNullOrEmpty(result.Edges[0].PathData), "Edge should have path data");
    }

    [TestMethod]
    public void ComputeLayout_TotalDimensions_ArePositive()
    {
        var unitA = CreateUnit("A");
        var unitB = CreateUnit("B", dependencies: [unitA]);

        var result = DagLayoutService.ComputeLayout([unitA, unitB]);

        Assert.IsTrue(result.TotalWidth > 0, "Total width should be positive");
        Assert.IsTrue(result.TotalHeight > 0, "Total height should be positive");
    }

    [TestMethod]
    public void ComputeLayout_IndependentNodes_AllGetPositions()
    {
        var unitA = CreateUnit("A");
        var unitB = CreateUnit("B");
        var unitC = CreateUnit("C");

        var result = DagLayoutService.ComputeLayout([unitA, unitB, unitC]);

        Assert.AreEqual(3, result.Nodes.Count);
        Assert.AreEqual(0, result.Edges.Count);

        // All nodes should have unique positions.
        var positions = result.Nodes.Select(n => (n.X, n.Y)).ToHashSet();
        Assert.AreEqual(3, positions.Count, "All nodes should have unique positions");
    }

    [TestMethod]
    public void ComputeLayout_PreservesUnitViewModelReference()
    {
        var unitA = CreateUnit("A", "Resource A");

        var result = DagLayoutService.ComputeLayout([unitA]);

        Assert.AreSame(unitA, result.Nodes[0].Unit);
        Assert.AreEqual("Resource A", result.Nodes[0].Label);
    }

    [TestMethod]
    public void ComputeLayout_WideFanOut_CreatesCorrectEdges()
    {
        var root = CreateUnit("root");
        var children = Enumerable.Range(1, 5)
            .Select(i => CreateUnit($"child{i}", dependencies: [root]))
            .ToList();

        var allUnits = new List<UnitViewModel> { root };
        allUnits.AddRange(children);

        var result = DagLayoutService.ComputeLayout(allUnits);

        Assert.AreEqual(6, result.Nodes.Count);
        Assert.AreEqual(5, result.Edges.Count);
    }
}

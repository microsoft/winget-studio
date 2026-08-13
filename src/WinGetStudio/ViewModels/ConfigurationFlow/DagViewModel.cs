// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Models.Graph;
using WinGetStudio.ViewModels;

namespace WinGetStudio.ViewModels.ConfigurationFlow;

/// <summary>
/// ViewModel for the DAG dependency graph visualization page.
/// Builds a visual graph from the active configuration set's units and their dependencies.
/// </summary>
public partial class DagViewModel : ObservableRecipient
{
    private readonly ILogger<DagViewModel> _logger;
    private readonly IConfigurationManager _manager;
    private readonly IConfigurationFrameNavigationService _navigationService;

    [ObservableProperty]
    public partial ObservableCollection<DagNode> Nodes { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<DagEdge> Edges { get; set; } = [];

    [ObservableProperty]
    public partial double CanvasWidth { get; set; }

    [ObservableProperty]
    public partial double CanvasHeight { get; set; }

    [ObservableProperty]
    public partial bool HasCycleError { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial bool IsEmpty { get; set; }

    public DagViewModel(
        ILogger<DagViewModel> logger,
        IConfigurationManager manager,
        IConfigurationFrameNavigationService navigationService)
    {
        _logger = logger;
        _manager = manager;
        _navigationService = navigationService;
    }

    [RelayCommand]
    private void OnLoaded()
    {
        BuildGraph();
    }

    [RelayCommand]
    private void OnBack()
    {
        _navigationService.NavigateToDefaultPage();
    }

    /// <summary>
    /// Toggles the selected state of a node and highlights its direct dependencies and dependents.
    /// </summary>
    /// <param name="selectedNode">The node that was tapped.</param>
    public void OnNodeSelected(DagNode selectedNode)
    {
        foreach (var node in Nodes)
        {
            node.IsSelected = node == selectedNode && !node.IsSelected;
            node.IsHighlighted = false;
        }

        foreach (var edge in Edges)
        {
            edge.IsHighlighted = false;
        }

        if (selectedNode.IsSelected)
        {
            // Highlight direct dependencies and dependents.
            foreach (var edge in Edges)
            {
                if (edge.Source == selectedNode || edge.Target == selectedNode)
                {
                    edge.IsHighlighted = true;
                    var otherNode = edge.Source == selectedNode ? edge.Target : edge.Source;
                    otherNode.IsHighlighted = true;
                }
            }
        }
    }

    /// <summary>
    /// Builds the DAG graph from the active configuration set's units and dependencies.
    /// Populates the Nodes and Edges collections with positioned elements.
    /// </summary>
    private void BuildGraph()
    {
        _logger.LogInformation("Building DAG visualization");
        Nodes.Clear();
        Edges.Clear();
        HasCycleError = false;
        ErrorMessage = null;

        var activePreviewSet = _manager.ActiveSetPreviewState.ActivePreviewSet;
        var configSet = activePreviewSet?.ConfigurationSet;

        if (configSet == null || configSet.Units.Count == 0)
        {
            IsEmpty = true;
            _logger.LogInformation("No configuration units to visualize");
            return;
        }

        IsEmpty = false;

        try
        {
            var result = DagLayoutService.ComputeLayout(configSet.Units.ToList());

            foreach (var node in result.Nodes)
            {
                Nodes.Add(node);
            }

            foreach (var edge in result.Edges)
            {
                Edges.Add(edge);
            }

            CanvasWidth = result.TotalWidth;
            CanvasHeight = result.TotalHeight;

            _logger.LogInformation("DAG visualization built with {NodeCount} nodes and {EdgeCount} edges", Nodes.Count, Edges.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compute DAG layout");
            HasCycleError = true;
            ErrorMessage = ex.Message;
        }
    }
}

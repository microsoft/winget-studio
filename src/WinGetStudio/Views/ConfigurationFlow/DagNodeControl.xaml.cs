// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using WinGetStudio.Models.Graph;

namespace WinGetStudio.Views.ConfigurationFlow;

/// <summary>
/// A control that renders a single node in the DAG visualization.
/// Displays the resource type icon, label, and ID with interactive selection and hover states.
/// </summary>
public sealed partial class DagNodeControl : UserControl
{
    private static readonly SolidColorBrush SelectedBorderBrush = new(Microsoft.UI.Colors.CornflowerBlue);
    private static readonly SolidColorBrush HighlightedBorderBrush = new(Microsoft.UI.Colors.Orange);

    public DagNode Node
    {
        get => (DagNode)GetValue(NodeProperty);
        set => SetValue(NodeProperty, value);
    }

    public static readonly DependencyProperty NodeProperty =
        DependencyProperty.Register(nameof(Node), typeof(DagNode), typeof(DagNodeControl), new PropertyMetadata(null, OnNodeChanged));

    public event EventHandler<DagNode>? NodeTapped;

    public DagNodeControl()
    {
        InitializeComponent();
    }

    private static void OnNodeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DagNodeControl control)
        {
            if (e.OldValue is DagNode oldNode)
            {
                oldNode.PropertyChanged -= control.OnNodePropertyChanged;
            }

            if (e.NewValue is DagNode newNode)
            {
                newNode.PropertyChanged += control.OnNodePropertyChanged;
                control.UpdateVisualState();
            }
        }
    }

    private void OnNodePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(DagNode.IsSelected) or nameof(DagNode.IsHighlighted))
        {
            DispatcherQueue.TryEnqueue(UpdateVisualState);
        }
    }

    /// <summary>
    /// Updates the border brush and thickness based on the node's selected/highlighted state.
    /// </summary>
    private void UpdateVisualState()
    {
        if (Node == null)
        {
            return;
        }

        if (Node.IsSelected)
        {
            NodeBorder.BorderBrush = SelectedBorderBrush;
            NodeBorder.BorderThickness = new Thickness(3);
        }
        else if (Node.IsHighlighted)
        {
            NodeBorder.BorderBrush = HighlightedBorderBrush;
            NodeBorder.BorderThickness = new Thickness(2.5);
        }
        else
        {
            NodeBorder.BorderBrush = (Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"];
            NodeBorder.BorderThickness = new Thickness(2);
        }
    }

    private void OnPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (!Node?.IsSelected ?? false)
        {
            NodeBorder.Background = (Brush)Application.Current.Resources["CardBackgroundFillColorSecondaryBrush"];
        }
    }

    private void OnPointerExited(object sender, PointerRoutedEventArgs e)
    {
        NodeBorder.Background = (Brush)Application.Current.Resources["CardBackgroundFillColorDefaultBrush"];
    }

    private void OnNodeTapped(object sender, TappedRoutedEventArgs e)
    {
        if (Node != null)
        {
            NodeTapped?.Invoke(this, Node);
        }
    }
}

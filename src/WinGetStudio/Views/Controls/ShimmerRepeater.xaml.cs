// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace WinGetStudio.Views.Controls;

[TemplatePart(Name = PartRepeater, Type = typeof(ItemsRepeater))]
public sealed partial class ShimmerRepeater : Control
{
    private const string PartRepeater = "PART_Repeater";
    private ItemsRepeater? _repeater;

    public static readonly DependencyProperty ItemCountProperty = DependencyProperty.Register(nameof(ItemCount), typeof(int), typeof(ShimmerRepeater), new PropertyMetadata(0, OnItemCountPropertyChanged));
    public static readonly DependencyProperty LayoutProperty = DependencyProperty.Register(nameof(Layout), typeof(Layout), typeof(ShimmerRepeater), new PropertyMetadata(null));
    public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(nameof(ItemTemplate), typeof(DataTemplate), typeof(ShimmerRepeater), new PropertyMetadata(null));

    public int ItemCount
    {
        get => (int)GetValue(ItemCountProperty);
        set => SetValue(ItemCountProperty, value);
    }

    public Layout Layout
    {
        get => (Layout)GetValue(LayoutProperty);
        set => SetValue(LayoutProperty, value);
    }

    public DataTemplate ItemTemplate
    {
        get => (DataTemplate)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public ShimmerRepeater()
    {
        DefaultStyleKey = typeof(ShimmerRepeater);
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _repeater = GetTemplateChild(PartRepeater) as ItemsRepeater;
        RefreshItems();
    }

    /// <summary>
    /// Handles changes to the ItemCount property.
    /// </summary>
    /// <param name="obj">The dependency object.</param>
    /// <param name="args">The event arguments.</param>
    private static void OnItemCountPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
        if (obj is ShimmerRepeater shimmerRepeater)
        {
            shimmerRepeater.RefreshItems();
        }
    }

    /// <summary>
    /// Refreshes the items in the ItemsRepeater.
    /// </summary>
    private void RefreshItems()
    {
        _repeater?.ItemsSource = Enumerable.Range(0, Math.Max(0, ItemCount));
    }
}

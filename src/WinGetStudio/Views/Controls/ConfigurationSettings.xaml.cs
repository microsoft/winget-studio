// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using WinGetStudio.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.Devices.Enumeration;

namespace WinGetStudio.Views.Controls;
public sealed partial class ConfigurationSettings : UserControl
{


    public static readonly DependencyProperty PropertiesProperty = DependencyProperty.Register(
        nameof(Properties),
        typeof(ObservableCollection<ConfigurationProperty>),
        typeof(ConfigurationSettings),
        new PropertyMetadata(null));
    

    public ObservableCollection<ConfigurationProperty> Properties
    {
        get => (ObservableCollection<ConfigurationProperty>)GetValue(PropertiesProperty);
        set => SetValue(PropertiesProperty, value);
    }
    public ConfigurationSettings()
    {
        InitializeComponent();
    }



    private void NewValidationPropertyDefault(SplitButton sender, SplitButtonClickEventArgs e)
    {
        ObservableCollection<ConfigurationProperty>? l = null;
        if (sender.DataContext is ConfigurationProperty p && p.Value.Value is ObservableCollection<ConfigurationProperty> l1)
        {
            l = l1;
        }
        else if (sender.DataContext is ConfigurationPropertyValueBase v && v.Value is ObservableCollection<ConfigurationProperty> l2)
        {
            l = l2;
        }
        l ??= Properties;
        if (l != null)
        {
            l.Add(new("", new StringValue("")));
        }
    }
    private void NewValidationProperty(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem item && item.Tag is string tag)
        {
            ObservableCollection<ConfigurationProperty>? l = null;
            if (item.DataContext is ConfigurationProperty p && p.Value.Value is ObservableCollection<ConfigurationProperty> l1)
            {
                l = l1;
            }
            else if (item.DataContext is ConfigurationPropertyValueBase v && v.Value is ObservableCollection<ConfigurationProperty> l2)
            {
                l = l2;
            }
            l ??= Properties;
            switch (tag)
            {
                case "Str":
                    l.Add(new("", new StringValue("")));
                    break;
                case "Num":
                    l.Add(new("", new NumberValue(0)));
                    break;
                case "Bool":
                    l.Add(new("", new BooleanValue(false)));
                    break;
                case "Arr":
                    l.Add(new("", new ArrayValue(new ObservableCollection<ConfigurationPropertyValueBase>())));
                    break;
                case "Obj":
                    l.Add(new("", new ObjectValue(new ObservableCollection<ConfigurationProperty>())));
                    break;
            }
        }
    }

    private void RemoveValidationProperty(object sender, RoutedEventArgs e)
    {

        var button = sender as Button;
        if (button != null)
        {
            var parent = VisualTreeHelper.GetParent(button);
            while (parent != null && parent is not ListView)
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            if (parent is ListView lv
                && lv.ItemsSource is ObservableCollection<ConfigurationProperty> l
                && button.DataContext is ConfigurationProperty p)
            {
                l.Remove(p);
            }
        }
    }

    private void ChangePropertyType(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem)
        {
            if (comboBox != null && comboBox.SelectedIndex == -1)
            {
                comboBox.SelectedIndex = 0;
            }

            if (comboBox.DataContext is ConfigurationProperty property)
            {
                var parent = VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(comboBox));
                while (parent != null && parent is not ListView)
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }
                ConfigurationPropertyValueBase newValue = null;
                switch (selectedItem.Tag)
                {
                    case "Str":
                        if(property.Value.Type != PropertyType.Object)
                        {
                            newValue = new StringValue(property.Value.Value.ToString());
                        }
                        else
                        {
                            newValue = new StringValue("");
                        }
                            break;
                    case "Bool":
                        try
                        {
                            newValue = new BooleanValue(bool.Parse(property.Value.Value.ToString()));
                        }
                        catch
                        {
                            newValue = new BooleanValue(false);
                        }
                        break;
                    case "Num":
                        try
                        {
                            newValue = new NumberValue(double.Parse(property.Value.Value.ToString()));
                        }
                        catch
                        {
                            newValue = new NumberValue(0);
                        }
                        break;
                    case "Obj":
                        newValue = new ObjectValue(new ObservableCollection<ConfigurationProperty>());
                        break;
                }
                if(parent is ListView lv
                    && lv.ItemsSource is ObservableCollection<ConfigurationProperty> l)
                {
                    if(property.Value.Type.ToString() != (comboBox.SelectedItem as ComboBoxItem).Content.ToString())
                    {
                        var i = lv.Items.IndexOf(property);

                        DispatcherQueue.TryEnqueue(() =>
                        {
                            switch (property.Value.Type)
                            {
                                case PropertyType.String:
                                    comboBox.SelectedIndex = 0;
                                    break;
                                case PropertyType.Boolean:
                                    comboBox.SelectedIndex = 1;
                                    break;
                                case PropertyType.Number:
                                    comboBox.SelectedIndex = 2;
                                    break;
                                case PropertyType.Object:
                                    comboBox.SelectedIndex = 3;
                                    break;
                            }
                            l[i] = new ConfigurationProperty(property.Name, newValue);
                        });
                    }
                }
            }
        }
    }
}

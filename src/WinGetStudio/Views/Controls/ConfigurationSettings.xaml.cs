// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.ObjectModel;
using System.Globalization;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WinGetStudio.Models;

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
        ObservableCollection<ConfigurationProperty>? listToUpdate = null;
        if (sender.DataContext is ConfigurationProperty property && property.Value.Value is ObservableCollection<ConfigurationProperty> nestedList)
        {
            listToUpdate = nestedList;
        }

        listToUpdate ??= Properties;
        if (listToUpdate != null)
        {
            listToUpdate.Add(new(string.Empty, new StringValue(string.Empty)));
        }
    }

    private void NewValidationProperty(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem item && item.Tag is string tag)
        {
            ObservableCollection<ConfigurationProperty>? listToUpdate = null;
            if (item.DataContext is ConfigurationProperty property && property.Value.Value is ObservableCollection<ConfigurationProperty> nestedList)
            {
                listToUpdate = nestedList;
            }

            listToUpdate ??= Properties;
            switch (tag)
            {
                case "Str":
                    listToUpdate.Add(new(string.Empty, new StringValue(string.Empty)));
                    break;
                case "Num":
                    listToUpdate.Add(new(string.Empty, new NumberValue(0)));
                    break;
                case "Bool":
                    listToUpdate.Add(new(string.Empty, new BooleanValue(false)));
                    break;
                case "Obj":
                    listToUpdate.Add(new(string.Empty, new ObjectValue(new ObservableCollection<ConfigurationProperty>())));
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

            if (parent is ListView listView
                && listView.ItemsSource is ObservableCollection<ConfigurationProperty> list
                && button.DataContext is ConfigurationProperty property)
            {
                list.Remove(property);
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

            if (comboBox?.DataContext is ConfigurationProperty property)
            {
                var parent = VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(comboBox));
                while (parent != null && parent is not ListView)
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }

                ConfigurationPropertyValueBase newValue;
                switch (selectedItem.Tag)
                {
                    case "Str":
                        if (property.Value.Type != PropertyType.ObjectType)
                        {
                            newValue = new StringValue(property.Value.Value.ToString()!);
                        }
                        else
                        {
                            newValue = new StringValue(string.Empty);
                        }

                        break;
                    case "Bool":
                        try
                        {
                            newValue = new BooleanValue(bool.Parse(property.Value.Value.ToString()!));
                        }
                        catch
                        {
                            newValue = new BooleanValue(false);
                        }

                        break;
                    case "Num":

                        try
                        {
                            newValue = new NumberValue(double.Parse(property.Value.Value.ToString()!, CultureInfo.InvariantCulture));
                        }
                        catch
                        {
                            newValue = new NumberValue(0);
                        }

                        break;
                    default:
                    case "Obj":
                        newValue = new ObjectValue(new ObservableCollection<ConfigurationProperty>());
                        break;
                }

                if (parent is ListView listView
                    && listView.ItemsSource is ObservableCollection<ConfigurationProperty> l)
                {
                    if (property.Value.Type.ToString() != (comboBox.SelectedItem as ComboBoxItem)?.Content.ToString())
                    {
                        var i = listView.Items.IndexOf(property);

                        DispatcherQueue.TryEnqueue(() =>
                        {
                            switch (property.Value.Type)
                            {
                                case PropertyType.StringType:
                                    comboBox.SelectedIndex = 0;
                                    break;
                                case PropertyType.BooleanType:
                                    comboBox.SelectedIndex = 1;
                                    break;
                                case PropertyType.NumberType:
                                    comboBox.SelectedIndex = 2;
                                    break;
                                case PropertyType.ObjectType:
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

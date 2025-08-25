// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace WinGetStudio.Selectors;
public class PropertyTemplateSelector : DataTemplateSelector
{
    public DataTemplate Number { get; set; } = new DataTemplate();
    public DataTemplate Boolean { get; set; } = new DataTemplate();
    public DataTemplate String { get; set; } = new DataTemplate();
    public DataTemplate Object { get; set; } = new DataTemplate();
    public DataTemplate Array { get; set; } = new DataTemplate();   

    protected override DataTemplate SelectTemplateCore(object item)
    {
        if (item is ConfigurationProperty p)
        {
            switch (p.Value.Type)
            {
                case PropertyType.Number:
                    return Number;
                case PropertyType.Boolean:
                    return Boolean;
                case PropertyType.String:
                    return String;
                case PropertyType.Object:
                    return Object;
            }
        }
        return Number;
    }
}
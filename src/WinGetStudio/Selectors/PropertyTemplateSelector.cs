// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinGetStudio.Models;

namespace WinGetStudio.Selectors;

public partial class PropertyTemplateSelector : DataTemplateSelector
{
    public DataTemplate NumberTemplate { get; set; } = new DataTemplate();

    public DataTemplate BooleanTemplate { get; set; } = new DataTemplate();

    public DataTemplate StringTemplate { get; set; } = new DataTemplate();

    public DataTemplate ObjectTemplate { get; set; } = new DataTemplate();

    public DataTemplate ArrayTemplate { get; set; } = new DataTemplate();

    protected override DataTemplate SelectTemplateCore(object item)
    {
        if (item is ConfigurationProperty p)
        {
            switch (p.Value.Type)
            {
                case PropertyType.NumberType:
                    return NumberTemplate;
                case PropertyType.BooleanType:
                    return BooleanTemplate;
                case PropertyType.StringType:
                    return StringTemplate;
                case PropertyType.ObjectType:
                    return ObjectTemplate;
            }
        }

        return NumberTemplate;
    }
}

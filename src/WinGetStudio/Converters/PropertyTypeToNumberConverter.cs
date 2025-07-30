// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.Models;
using Microsoft.UI.Xaml.Data;

namespace WinGetStudio.Converters;
public class PropertyTypeToNumberConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is PropertyType p)
        {
            switch (p)
            {
                case PropertyType.String:
                    return 0;
                case PropertyType.Boolean:
                    return 1;
                case PropertyType.Number:
                    return 2;
                case PropertyType.Object:
                    return 3;
            }
        }
        return 0;
    }


    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is double d)
        {
            switch ((int)d)
            {
                case 0:
                    return PropertyType.String;
                case 1:
                    return PropertyType.Number;
                case 2:
                    return PropertyType.Boolean;
                case 3:
                    return PropertyType.Object;
            }
        }
        return PropertyType.String;
    }
}

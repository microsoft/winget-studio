// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.UI.Xaml.Data;
using WinGetStudio.Models;

namespace WinGetStudio.Converters;

public partial class PropertyTypeToNumberConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is PropertyType p)
        {
            switch (p)
            {
                case PropertyType.StringType:
                    return 0;
                case PropertyType.BooleanType:
                    return 1;
                case PropertyType.NumberType:
                    return 2;
                case PropertyType.ObjectType:
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
                    return PropertyType.StringType;
                case 1:
                    return PropertyType.NumberType;
                case 2:
                    return PropertyType.BooleanType;
                case 3:
                    return PropertyType.ObjectType;
            }
        }

        return PropertyType.StringType;
    }
}


using System;
using Microsoft.UI.Xaml.Data;
using System.Globalization;

namespace WinGetStudio.Converters;
public class DivideByConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is double doubleValue && parameter != null)
        {
            if (double.TryParse(parameter.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double divisor) && divisor != 0)
            {
                return doubleValue / divisor;
            }
            return doubleValue / 2;
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException(); // Implement if needed
    }
}

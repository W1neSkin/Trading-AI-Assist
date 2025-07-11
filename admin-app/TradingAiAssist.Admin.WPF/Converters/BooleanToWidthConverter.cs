using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TradingAiAssist.Admin.WPF.Converters
{
    public class BooleanToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string paramString)
            {
                var parts = paramString.Split(',');
                if (parts.Length == 2 && 
                    double.TryParse(parts[0], out double expandedWidth) && 
                    double.TryParse(parts[1], out double collapsedWidth))
                {
                    return boolValue ? expandedWidth : collapsedWidth;
                }
            }
            return 280.0; // Default width
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 
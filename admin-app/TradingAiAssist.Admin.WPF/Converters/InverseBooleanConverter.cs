using System;
using System.Globalization;
using System.Windows.Data;

namespace TradingAiAssist.Admin.WPF.Converters
{
    /// <summary>
    /// Inverts a boolean value. True becomes False and vice versa.
    /// </summary>
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            
            return false;
        }
    }
} 
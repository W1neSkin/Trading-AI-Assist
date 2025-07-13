using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TradingAiAssist.Admin.WPF.Converters
{
    /// <summary>
    /// Converts destructive state to button style
    /// </summary>
    public class ButtonStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isDestructive)
            {
                return isDestructive ? "DestructiveButton" : "ModernButton";
            }
            
            return "ModernButton";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 
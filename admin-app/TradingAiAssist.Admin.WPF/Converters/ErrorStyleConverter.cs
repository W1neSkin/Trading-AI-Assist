using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace TradingAiAssist.Admin.WPF.Converters
{
    /// <summary>
    /// Converts error state to different styling values based on the parameter.
    /// Parameters: "ErrorBackground", "ErrorBorder", "ErrorText"
    /// </summary>
    public class ErrorStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isError && parameter is string param)
            {
                switch (param.ToLower())
                {
                    case "errorbackground":
                        return isError ? new SolidColorBrush(Color.FromRgb(255, 235, 235)) : new SolidColorBrush(Color.FromRgb(235, 255, 235));
                    
                    case "errorborder":
                        return isError ? new SolidColorBrush(Color.FromRgb(255, 100, 100)) : new SolidColorBrush(Color.FromRgb(100, 255, 100));
                    
                    case "errortext":
                        return isError ? new SolidColorBrush(Color.FromRgb(200, 0, 0)) : new SolidColorBrush(Color.FromRgb(0, 150, 0));
                    
                    default:
                        return new SolidColorBrush(Colors.Transparent);
                }
            }
            
            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 
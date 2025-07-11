using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TradingAiAssist.Admin.WPF.Converters
{
    public class ConnectionStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status.ToLower() switch
                {
                    "connected" or "healthy" => new SolidColorBrush(Colors.Green),
                    "connecting" or "warning" => new SolidColorBrush(Colors.Orange),
                    "disconnected" or "error" or "critical" => new SolidColorBrush(Colors.Red),
                    "offline" => new SolidColorBrush(Colors.Gray),
                    _ => new SolidColorBrush(Colors.Gray)
                };
            }

            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 
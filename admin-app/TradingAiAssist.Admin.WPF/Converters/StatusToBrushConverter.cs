using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using TradingAiAssist.Admin.Core.Models;

namespace TradingAiAssist.Admin.WPF.Converters
{
    /// <summary>
    /// Converts service status values to appropriate brush colors for UI display
    /// </summary>
    public class StatusToBrushConverter : IValueConverter
    {
        /// <summary>
        /// Converts a service status to a brush color
        /// </summary>
        /// <param name="value">The status value to convert</param>
        /// <param name="targetType">The target type (Brush)</param>
        /// <param name="parameter">Optional parameter</param>
        /// <param name="culture">Culture information</param>
        /// <returns>A brush representing the status color</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return new SolidColorBrush(Colors.Gray);

            // Handle string status values
            if (value is string statusString)
            {
                return GetBrushFromString(statusString);
            }

            // Handle ServiceStatus enum values
            if (value is ServiceStatus serviceStatus)
            {
                return GetBrushFromServiceStatus(serviceStatus);
            }

            // Handle SystemHealthStatus enum values
            if (value is SystemHealthStatus healthStatus)
            {
                return GetBrushFromHealthStatus(healthStatus);
            }

            // Default to gray for unknown values
            return new SolidColorBrush(Colors.Gray);
        }

        /// <summary>
        /// Converts a brush back to a status value (not implemented)
        /// </summary>
        /// <param name="value">The brush value</param>
        /// <param name="targetType">The target type</param>
        /// <param name="parameter">Optional parameter</param>
        /// <param name="culture">Culture information</param>
        /// <returns>Not implemented</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("StatusToBrushConverter does not support ConvertBack");
        }

        /// <summary>
        /// Gets a brush color from a string status value
        /// </summary>
        /// <param name="status">The status string</param>
        /// <returns>A brush color</returns>
        private SolidColorBrush GetBrushFromString(string status)
        {
            return status?.ToLowerInvariant() switch
            {
                "online" or "healthy" or "success" or "active" or "running" => new SolidColorBrush(Color.FromRgb(76, 175, 80)), // Green
                "offline" or "unhealthy" or "error" or "failed" or "stopped" => new SolidColorBrush(Color.FromRgb(244, 67, 54)), // Red
                "warning" or "degraded" or "partial" or "maintenance" => new SolidColorBrush(Color.FromRgb(255, 152, 0)), // Orange
                "info" or "pending" or "initializing" or "starting" => new SolidColorBrush(Color.FromRgb(33, 150, 243)), // Blue
                "unknown" or "n/a" or "not available" => new SolidColorBrush(Color.FromRgb(158, 158, 158)), // Gray
                _ => new SolidColorBrush(Colors.Gray)
            };
        }

        /// <summary>
        /// Gets a brush color from a ServiceStatus enum value
        /// </summary>
        /// <param name="status">The service status</param>
        /// <returns>A brush color</returns>
        private SolidColorBrush GetBrushFromServiceStatus(ServiceStatus status)
        {
            return status switch
            {
                ServiceStatus.Online => new SolidColorBrush(Color.FromRgb(76, 175, 80)), // Green
                ServiceStatus.Offline => new SolidColorBrush(Color.FromRgb(244, 67, 54)), // Red
                ServiceStatus.Degraded => new SolidColorBrush(Color.FromRgb(255, 152, 0)), // Orange
                ServiceStatus.Maintenance => new SolidColorBrush(Color.FromRgb(33, 150, 243)), // Blue
                _ => new SolidColorBrush(Colors.Gray)
            };
        }

        /// <summary>
        /// Gets a brush color from a SystemHealthStatus enum value
        /// </summary>
        /// <param name="status">The health status</param>
        /// <returns>A brush color</returns>
        private SolidColorBrush GetBrushFromHealthStatus(SystemHealthStatus status)
        {
            return status switch
            {
                SystemHealthStatus.Healthy => new SolidColorBrush(Color.FromRgb(76, 175, 80)), // Green
                SystemHealthStatus.Unhealthy => new SolidColorBrush(Color.FromRgb(244, 67, 54)), // Red
                SystemHealthStatus.Warning => new SolidColorBrush(Color.FromRgb(255, 152, 0)), // Orange
                SystemHealthStatus.Unknown => new SolidColorBrush(Colors.Gray),
                _ => new SolidColorBrush(Colors.Gray)
            };
        }
    }
} 
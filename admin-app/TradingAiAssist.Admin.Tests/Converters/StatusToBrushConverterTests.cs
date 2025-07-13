using System;
using System.Globalization;
using System.Windows.Media;
using FluentAssertions;
using TradingAiAssist.Admin.Core.Models;
using TradingAiAssist.Admin.WPF.Converters;
using Xunit;

namespace TradingAiAssist.Admin.Tests.Converters
{
    /// <summary>
    /// Unit tests for StatusToBrushConverter
    /// </summary>
    public class StatusToBrushConverterTests
    {
        private readonly StatusToBrushConverter _converter;

        public StatusToBrushConverterTests()
        {
            _converter = new StatusToBrushConverter();
        }

        [Fact]
        public void Convert_NullValue_ReturnsGrayBrush()
        {
            // Act
            var result = _converter.Convert(null, typeof(Brush), null, CultureInfo.InvariantCulture);

            // Assert
            result.Should().BeOfType<SolidColorBrush>();
            ((SolidColorBrush)result).Color.Should().Be(Colors.Gray);
        }

        [Theory]
        [InlineData("online", 76, 175, 80)]      // Green
        [InlineData("healthy", 76, 175, 80)]     // Green
        [InlineData("success", 76, 175, 80)]     // Green
        [InlineData("active", 76, 175, 80)]      // Green
        [InlineData("running", 76, 175, 80)]     // Green
        public void Convert_OnlineStatusStrings_ReturnsGreenBrush(string status, byte r, byte g, byte b)
        {
            // Act
            var result = _converter.Convert(status, typeof(Brush), null, CultureInfo.InvariantCulture);

            // Assert
            result.Should().BeOfType<SolidColorBrush>();
            var brush = (SolidColorBrush)result;
            brush.Color.R.Should().Be(r);
            brush.Color.G.Should().Be(g);
            brush.Color.B.Should().Be(b);
        }

        [Theory]
        [InlineData("offline", 244, 67, 54)]     // Red
        [InlineData("unhealthy", 244, 67, 54)]   // Red
        [InlineData("error", 244, 67, 54)]       // Red
        [InlineData("failed", 244, 67, 54)]      // Red
        [InlineData("stopped", 244, 67, 54)]     // Red
        public void Convert_OfflineStatusStrings_ReturnsRedBrush(string status, byte r, byte g, byte b)
        {
            // Act
            var result = _converter.Convert(status, typeof(Brush), null, CultureInfo.InvariantCulture);

            // Assert
            result.Should().BeOfType<SolidColorBrush>();
            var brush = (SolidColorBrush)result;
            brush.Color.R.Should().Be(r);
            brush.Color.G.Should().Be(g);
            brush.Color.B.Should().Be(b);
        }

        [Theory]
        [InlineData("warning", 255, 152, 0)]     // Orange
        [InlineData("degraded", 255, 152, 0)]    // Orange
        [InlineData("partial", 255, 152, 0)]     // Orange
        [InlineData("maintenance", 255, 152, 0)] // Orange
        public void Convert_WarningStatusStrings_ReturnsOrangeBrush(string status, byte r, byte g, byte b)
        {
            // Act
            var result = _converter.Convert(status, typeof(Brush), null, CultureInfo.InvariantCulture);

            // Assert
            result.Should().BeOfType<SolidColorBrush>();
            var brush = (SolidColorBrush)result;
            brush.Color.R.Should().Be(r);
            brush.Color.G.Should().Be(g);
            brush.Color.B.Should().Be(b);
        }

        [Theory]
        [InlineData("info", 33, 150, 243)]       // Blue
        [InlineData("pending", 33, 150, 243)]    // Blue
        [InlineData("initializing", 33, 150, 243)] // Blue
        [InlineData("starting", 33, 150, 243)]   // Blue
        public void Convert_InfoStatusStrings_ReturnsBlueBrush(string status, byte r, byte g, byte b)
        {
            // Act
            var result = _converter.Convert(status, typeof(Brush), null, CultureInfo.InvariantCulture);

            // Assert
            result.Should().BeOfType<SolidColorBrush>();
            var brush = (SolidColorBrush)result;
            brush.Color.R.Should().Be(r);
            brush.Color.G.Should().Be(g);
            brush.Color.B.Should().Be(b);
        }

        [Theory]
        [InlineData("unknown")]
        [InlineData("n/a")]
        [InlineData("not available")]
        [InlineData("invalid")]
        public void Convert_UnknownStatusStrings_ReturnsGrayBrush(string status)
        {
            // Act
            var result = _converter.Convert(status, typeof(Brush), null, CultureInfo.InvariantCulture);

            // Assert
            result.Should().BeOfType<SolidColorBrush>();
            ((SolidColorBrush)result).Color.Should().Be(Colors.Gray);
        }

        [Theory]
        [InlineData(ServiceStatus.Online, 76, 175, 80)]      // Green
        [InlineData(ServiceStatus.Offline, 244, 67, 54)]     // Red
        [InlineData(ServiceStatus.Degraded, 255, 152, 0)]    // Orange
        [InlineData(ServiceStatus.Maintenance, 33, 150, 243)] // Blue
        public void Convert_ServiceStatusEnum_ReturnsCorrectBrush(ServiceStatus status, byte r, byte g, byte b)
        {
            // Act
            var result = _converter.Convert(status, typeof(Brush), null, CultureInfo.InvariantCulture);

            // Assert
            result.Should().BeOfType<SolidColorBrush>();
            var brush = (SolidColorBrush)result;
            brush.Color.R.Should().Be(r);
            brush.Color.G.Should().Be(g);
            brush.Color.B.Should().Be(b);
        }

        [Theory]
        [InlineData(SystemHealthStatus.Healthy, 76, 175, 80)]      // Green
        [InlineData(SystemHealthStatus.Unhealthy, 244, 67, 54)]   // Red
        [InlineData(SystemHealthStatus.Warning, 255, 152, 0)]     // Orange
        [InlineData(SystemHealthStatus.Unknown, 158, 158, 158)]   // Gray
        public void Convert_SystemHealthStatusEnum_ReturnsCorrectBrush(SystemHealthStatus status, byte r, byte g, byte b)
        {
            // Act
            var result = _converter.Convert(status, typeof(Brush), null, CultureInfo.InvariantCulture);

            // Assert
            result.Should().BeOfType<SolidColorBrush>();
            var brush = (SolidColorBrush)result;
            brush.Color.R.Should().Be(r);
            brush.Color.G.Should().Be(g);
            brush.Color.B.Should().Be(b);
        }

        [Fact]
        public void ConvertBack_ThrowsNotImplementedException()
        {
            // Act & Assert
            Action act = () => _converter.ConvertBack(new SolidColorBrush(Colors.Red), typeof(string), null, CultureInfo.InvariantCulture);
            act.Should().Throw<NotImplementedException>().WithMessage("StatusToBrushConverter does not support ConvertBack");
        }
    }
} 
using System;
using FluentAssertions;
using TradingAiAssist.Admin.WPF.Helpers;
using Xunit;

namespace TradingAiAssist.Admin.Tests.Helpers
{
    /// <summary>
    /// Unit tests for DateTimeHelper
    /// </summary>
    public class DateTimeHelperTests
    {
        [Fact]
        public void FormatDateTime_ValidDateTime_ReturnsFormattedString()
        {
            // Arrange
            var dateTime = new DateTime(2024, 1, 15, 14, 30, 45);

            // Act
            var result = DateTimeHelper.FormatDateTime(dateTime);

            // Assert
            result.Should().Be("Jan 15, 2024 2:30 PM");
        }

        [Fact]
        public void FormatDateTime_NullDateTime_ReturnsEmptyString()
        {
            // Act
            var result = DateTimeHelper.FormatDateTime(null);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void FormatDate_ValidDateTime_ReturnsFormattedString()
        {
            // Arrange
            var dateTime = new DateTime(2024, 1, 15);

            // Act
            var result = DateTimeHelper.FormatDate(dateTime);

            // Assert
            result.Should().Be("Jan 15, 2024");
        }

        [Fact]
        public void FormatDate_NullDateTime_ReturnsEmptyString()
        {
            // Act
            var result = DateTimeHelper.FormatDate(null);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void FormatTime_ValidDateTime_ReturnsFormattedString()
        {
            // Arrange
            var dateTime = new DateTime(2024, 1, 15, 14, 30, 45);

            // Act
            var result = DateTimeHelper.FormatTime(dateTime);

            // Assert
            result.Should().Be("2:30 PM");
        }

        [Fact]
        public void FormatTime_NullDateTime_ReturnsEmptyString()
        {
            // Act
            var result = DateTimeHelper.FormatTime(null);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void FormatRelativeTime_Now_ReturnsJustNow()
        {
            // Arrange
            var now = DateTime.Now;

            // Act
            var result = DateTimeHelper.FormatRelativeTime(now);

            // Assert
            result.Should().Be("Just now");
        }

        [Fact]
        public void FormatRelativeTime_OneMinuteAgo_ReturnsOneMinuteAgo()
        {
            // Arrange
            var oneMinuteAgo = DateTime.Now.AddMinutes(-1);

            // Act
            var result = DateTimeHelper.FormatRelativeTime(oneMinuteAgo);

            // Assert
            result.Should().Be("1 minute ago");
        }

        [Fact]
        public void FormatRelativeTime_FiveMinutesAgo_ReturnsFiveMinutesAgo()
        {
            // Arrange
            var fiveMinutesAgo = DateTime.Now.AddMinutes(-5);

            // Act
            var result = DateTimeHelper.FormatRelativeTime(fiveMinutesAgo);

            // Assert
            result.Should().Be("5 minutes ago");
        }

        [Fact]
        public void FormatRelativeTime_OneHourAgo_ReturnsOneHourAgo()
        {
            // Arrange
            var oneHourAgo = DateTime.Now.AddHours(-1);

            // Act
            var result = DateTimeHelper.FormatRelativeTime(oneHourAgo);

            // Assert
            result.Should().Be("1 hour ago");
        }

        [Fact]
        public void FormatRelativeTime_TwoHoursAgo_ReturnsTwoHoursAgo()
        {
            // Arrange
            var twoHoursAgo = DateTime.Now.AddHours(-2);

            // Act
            var result = DateTimeHelper.FormatRelativeTime(twoHoursAgo);

            // Assert
            result.Should().Be("2 hours ago");
        }

        [Fact]
        public void FormatRelativeTime_OneDayAgo_ReturnsOneDayAgo()
        {
            // Arrange
            var oneDayAgo = DateTime.Now.AddDays(-1);

            // Act
            var result = DateTimeHelper.FormatRelativeTime(oneDayAgo);

            // Assert
            result.Should().Be("1 day ago");
        }

        [Fact]
        public void FormatRelativeTime_OneWeekAgo_ReturnsOneWeekAgo()
        {
            // Arrange
            var oneWeekAgo = DateTime.Now.AddDays(-7);

            // Act
            var result = DateTimeHelper.FormatRelativeTime(oneWeekAgo);

            // Assert
            result.Should().Be("1 week ago");
        }

        [Fact]
        public void FormatRelativeTime_OneMonthAgo_ReturnsOneMonthAgo()
        {
            // Arrange
            var oneMonthAgo = DateTime.Now.AddMonths(-1);

            // Act
            var result = DateTimeHelper.FormatRelativeTime(oneMonthAgo);

            // Assert
            result.Should().Contain("month ago");
        }

        [Fact]
        public void FormatRelativeTime_OneYearAgo_ReturnsOneYearAgo()
        {
            // Arrange
            var oneYearAgo = DateTime.Now.AddYears(-1);

            // Act
            var result = DateTimeHelper.FormatRelativeTime(oneYearAgo);

            // Assert
            result.Should().Contain("year ago");
        }

        [Fact]
        public void FormatRelativeTime_NullDateTime_ReturnsEmptyString()
        {
            // Act
            var result = DateTimeHelper.FormatRelativeTime(null);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void FormatDuration_ZeroSeconds_ReturnsZeroSeconds()
        {
            // Arrange
            var duration = TimeSpan.Zero;

            // Act
            var result = DateTimeHelper.FormatDuration(duration);

            // Assert
            result.Should().Be("0 seconds");
        }

        [Fact]
        public void FormatDuration_ThirtySeconds_ReturnsThirtySeconds()
        {
            // Arrange
            var duration = TimeSpan.FromSeconds(30);

            // Act
            var result = DateTimeHelper.FormatDuration(duration);

            // Assert
            result.Should().Be("30 seconds");
        }

        [Fact]
        public void FormatDuration_OneMinute_ReturnsOneMinute()
        {
            // Arrange
            var duration = TimeSpan.FromMinutes(1);

            // Act
            var result = DateTimeHelper.FormatDuration(duration);

            // Assert
            result.Should().Be("1 minute");
        }

        [Fact]
        public void FormatDuration_OneHour_ReturnsOneHour()
        {
            // Arrange
            var duration = TimeSpan.FromHours(1);

            // Act
            var result = DateTimeHelper.FormatDuration(duration);

            // Assert
            result.Should().Be("1 hour");
        }

        [Fact]
        public void FormatDuration_OneDay_ReturnsOneDay()
        {
            // Arrange
            var duration = TimeSpan.FromDays(1);

            // Act
            var result = DateTimeHelper.FormatDuration(duration);

            // Assert
            result.Should().Be("1 day");
        }

        [Fact]
        public void FormatDuration_ComplexDuration_ReturnsFormattedString()
        {
            // Arrange
            var duration = new TimeSpan(2, 3, 45, 30); // 2 days, 3 hours, 45 minutes, 30 seconds

            // Act
            var result = DateTimeHelper.FormatDuration(duration);

            // Assert
            result.Should().Be("2 days, 3 hours, 45 minutes");
        }
    }
} 
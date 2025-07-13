using System;
using System.Globalization;
using FluentAssertions;
using TradingAiAssist.Admin.WPF.Helpers;
using Xunit;

namespace TradingAiAssist.Admin.Tests.Helpers
{
    /// <summary>
    /// Unit tests for CurrencyHelper
    /// </summary>
    public class CurrencyHelperTests
    {
        [Fact]
        public void FormatCurrency_ValidAmount_ReturnsFormattedString()
        {
            // Arrange
            var amount = 1234.56m;

            // Act
            var result = CurrencyHelper.FormatCurrency(amount);

            // Assert
            result.Should().Be("$1,234.56");
        }

        [Fact]
        public void FormatCurrency_ZeroAmount_ReturnsFormattedString()
        {
            // Arrange
            var amount = 0m;

            // Act
            var result = CurrencyHelper.FormatCurrency(amount);

            // Assert
            result.Should().Be("$0.00");
        }

        [Fact]
        public void FormatCurrency_NegativeAmount_ReturnsFormattedString()
        {
            // Arrange
            var amount = -1234.56m;

            // Act
            var result = CurrencyHelper.FormatCurrency(amount);

            // Assert
            result.Should().Be("-$1,234.56");
        }

        [Fact]
        public void FormatCurrency_LargeAmount_ReturnsFormattedString()
        {
            // Arrange
            var amount = 1234567.89m;

            // Act
            var result = CurrencyHelper.FormatCurrency(amount);

            // Assert
            result.Should().Be("$1,234,567.89");
        }

        [Fact]
        public void FormatCurrency_WithCustomCurrency_ReturnsFormattedString()
        {
            // Arrange
            var amount = 1234.56m;
            var currency = "EUR";

            // Act
            var result = CurrencyHelper.FormatCurrency(amount, currency);

            // Assert
            result.Should().Be("€1,234.56");
        }

        [Fact]
        public void FormatCurrency_WithCustomCulture_ReturnsFormattedString()
        {
            // Arrange
            var amount = 1234.56m;
            var culture = new CultureInfo("en-GB");

            // Act
            var result = CurrencyHelper.FormatCurrency(amount, culture: culture);

            // Assert
            result.Should().Be("£1,234.56");
        }

        [Fact]
        public void FormatCurrency_WithCustomCurrencyAndCulture_ReturnsFormattedString()
        {
            // Arrange
            var amount = 1234.56m;
            var currency = "EUR";
            var culture = new CultureInfo("de-DE");

            // Act
            var result = CurrencyHelper.FormatCurrency(amount, currency, culture);

            // Assert
            result.Should().Be("1.234,56 €");
        }

        [Fact]
        public void FormatPercentage_ValidPercentage_ReturnsFormattedString()
        {
            // Arrange
            var percentage = 12.34m;

            // Act
            var result = CurrencyHelper.FormatPercentage(percentage);

            // Assert
            result.Should().Be("12.34%");
        }

        [Fact]
        public void FormatPercentage_ZeroPercentage_ReturnsFormattedString()
        {
            // Arrange
            var percentage = 0m;

            // Act
            var result = CurrencyHelper.FormatPercentage(percentage);

            // Assert
            result.Should().Be("0.00%");
        }

        [Fact]
        public void FormatPercentage_NegativePercentage_ReturnsFormattedString()
        {
            // Arrange
            var percentage = -12.34m;

            // Act
            var result = CurrencyHelper.FormatPercentage(percentage);

            // Assert
            result.Should().Be("-12.34%");
        }

        [Fact]
        public void FormatPercentage_WithCustomDecimals_ReturnsFormattedString()
        {
            // Arrange
            var percentage = 12.3456m;
            var decimals = 2;

            // Act
            var result = CurrencyHelper.FormatPercentage(percentage, decimals);

            // Assert
            result.Should().Be("12.35%");
        }

        [Fact]
        public void FormatNumber_ValidNumber_ReturnsFormattedString()
        {
            // Arrange
            var number = 1234.56m;

            // Act
            var result = CurrencyHelper.FormatNumber(number);

            // Assert
            result.Should().Be("1,234.56");
        }

        [Fact]
        public void FormatNumber_ZeroNumber_ReturnsFormattedString()
        {
            // Arrange
            var number = 0m;

            // Act
            var result = CurrencyHelper.FormatNumber(number);

            // Assert
            result.Should().Be("0.00");
        }

        [Fact]
        public void FormatNumber_WithCustomDecimals_ReturnsFormattedString()
        {
            // Arrange
            var number = 1234.5678m;
            var decimals = 3;

            // Act
            var result = CurrencyHelper.FormatNumber(number, decimals);

            // Assert
            result.Should().Be("1,234.568");
        }

        [Fact]
        public void FormatNumber_WithCustomCulture_ReturnsFormattedString()
        {
            // Arrange
            var number = 1234.56m;
            var culture = new CultureInfo("de-DE");

            // Act
            var result = CurrencyHelper.FormatNumber(number, culture: culture);

            // Assert
            result.Should().Be("1.234,56");
        }

        [Fact]
        public void FormatCompactNumber_Thousand_ReturnsFormattedString()
        {
            // Arrange
            var number = 1234m;

            // Act
            var result = CurrencyHelper.FormatCompactNumber(number);

            // Assert
            result.Should().Be("1.23K");
        }

        [Fact]
        public void FormatCompactNumber_Million_ReturnsFormattedString()
        {
            // Arrange
            var number = 1234567m;

            // Act
            var result = CurrencyHelper.FormatCompactNumber(number);

            // Assert
            result.Should().Be("1.23M");
        }

        [Fact]
        public void FormatCompactNumber_Billion_ReturnsFormattedString()
        {
            // Arrange
            var number = 1234567890m;

            // Act
            var result = CurrencyHelper.FormatCompactNumber(number);

            // Assert
            result.Should().Be("1.23B");
        }

        [Fact]
        public void FormatCompactNumber_SmallNumber_ReturnsFormattedString()
        {
            // Arrange
            var number = 123m;

            // Act
            var result = CurrencyHelper.FormatCompactNumber(number);

            // Assert
            result.Should().Be("123");
        }

        [Fact]
        public void FormatCompactCurrency_Thousand_ReturnsFormattedString()
        {
            // Arrange
            var amount = 1234m;

            // Act
            var result = CurrencyHelper.FormatCompactCurrency(amount);

            // Assert
            result.Should().Be("$1.23K");
        }

        [Fact]
        public void FormatCompactCurrency_Million_ReturnsFormattedString()
        {
            // Arrange
            var amount = 1234567m;

            // Act
            var result = CurrencyHelper.FormatCompactCurrency(amount);

            // Assert
            result.Should().Be("$1.23M");
        }

        [Fact]
        public void FormatCompactCurrency_Billion_ReturnsFormattedString()
        {
            // Arrange
            var amount = 1234567890m;

            // Act
            var result = CurrencyHelper.FormatCompactCurrency(amount);

            // Assert
            result.Should().Be("$1.23B");
        }

        [Fact]
        public void ParseCurrency_ValidString_ReturnsDecimal()
        {
            // Arrange
            var currencyString = "$1,234.56";

            // Act
            var result = CurrencyHelper.ParseCurrency(currencyString);

            // Assert
            result.Should().Be(1234.56m);
        }

        [Fact]
        public void ParseCurrency_InvalidString_ReturnsZero()
        {
            // Arrange
            var currencyString = "invalid";

            // Act
            var result = CurrencyHelper.ParseCurrency(currencyString);

            // Assert
            result.Should().Be(0m);
        }

        [Fact]
        public void ParseCurrency_EmptyString_ReturnsZero()
        {
            // Arrange
            var currencyString = "";

            // Act
            var result = CurrencyHelper.ParseCurrency(currencyString);

            // Assert
            result.Should().Be(0m);
        }

        [Fact]
        public void ParseCurrency_NullString_ReturnsZero()
        {
            // Act
            var result = CurrencyHelper.ParseCurrency(null);

            // Assert
            result.Should().Be(0m);
        }

        [Fact]
        public void ParsePercentage_ValidString_ReturnsDecimal()
        {
            // Arrange
            var percentageString = "12.34%";

            // Act
            var result = CurrencyHelper.ParsePercentage(percentageString);

            // Assert
            result.Should().Be(12.34m);
        }

        [Fact]
        public void ParsePercentage_InvalidString_ReturnsZero()
        {
            // Arrange
            var percentageString = "invalid";

            // Act
            var result = CurrencyHelper.ParsePercentage(percentageString);

            // Assert
            result.Should().Be(0m);
        }

        [Fact]
        public void ParsePercentage_EmptyString_ReturnsZero()
        {
            // Arrange
            var percentageString = "";

            // Act
            var result = CurrencyHelper.ParsePercentage(percentageString);

            // Assert
            result.Should().Be(0m);
        }

        [Fact]
        public void ParsePercentage_NullString_ReturnsZero()
        {
            // Act
            var result = CurrencyHelper.ParsePercentage(null);

            // Assert
            result.Should().Be(0m);
        }
    }
} 
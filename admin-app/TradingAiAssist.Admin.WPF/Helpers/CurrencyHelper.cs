using System;
using System.Globalization;

namespace TradingAiAssist.Admin.WPF.Helpers
{
    public static class CurrencyHelper
    {
        public static string FormatCurrency(decimal amount, string currencyCode = "USD")
        {
            var culture = GetCultureInfo(currencyCode);
            return amount.ToString("C", culture);
        }

        public static string FormatCurrencyCompact(decimal amount, string currencyCode = "USD")
        {
            if (Math.Abs(amount) >= 1000000)
            {
                return $"{amount / 1000000:F1}M";
            }
            else if (Math.Abs(amount) >= 1000)
            {
                return $"{amount / 1000:F1}K";
            }
            else
            {
                return FormatCurrency(amount, currencyCode);
            }
        }

        public static string FormatPercentage(decimal value, int decimalPlaces = 1)
        {
            return $"{value:F{decimalPlaces}}%";
        }

        public static string FormatPercentageChange(decimal change, bool showSign = true)
        {
            var sign = showSign && change > 0 ? "+" : "";
            return $"{sign}{change:F1}%";
        }

        public static string GetCurrencySymbol(string currencyCode = "USD")
        {
            var culture = GetCultureInfo(currencyCode);
            return culture.NumberFormat.CurrencySymbol;
        }

        public static decimal CalculatePercentageChange(decimal oldValue, decimal newValue)
        {
            if (oldValue == 0)
                return newValue > 0 ? 100 : 0;

            return ((newValue - oldValue) / oldValue) * 100;
        }

        public static decimal CalculateGrowthRate(decimal initialValue, decimal finalValue, int periods = 1)
        {
            if (initialValue <= 0 || periods <= 0)
                return 0;

            return (decimal)(Math.Pow((double)(finalValue / initialValue), 1.0 / periods) - 1) * 100;
        }

        public static string FormatNumber(decimal number, int decimalPlaces = 0)
        {
            return number.ToString($"N{decimalPlaces}");
        }

        public static string FormatNumberCompact(decimal number)
        {
            if (Math.Abs(number) >= 1000000)
            {
                return $"{number / 1000000:F1}M";
            }
            else if (Math.Abs(number) >= 1000)
            {
                return $"{number / 1000:F1}K";
            }
            else
            {
                return number.ToString("N0");
            }
        }

        public static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        public static string FormatDuration(TimeSpan duration)
        {
            if (duration.TotalDays >= 1)
            {
                return $"{(int)duration.TotalDays}d {duration.Hours}h {duration.Minutes}m";
            }
            else if (duration.TotalHours >= 1)
            {
                return $"{duration.Hours}h {duration.Minutes}m {duration.Seconds}s";
            }
            else if (duration.TotalMinutes >= 1)
            {
                return $"{duration.Minutes}m {duration.Seconds}s";
            }
            else
            {
                return $"{duration.Seconds}s";
            }
        }

        private static CultureInfo GetCultureInfo(string currencyCode)
        {
            return currencyCode.ToUpper() switch
            {
                "USD" => new CultureInfo("en-US"),
                "EUR" => new CultureInfo("de-DE"),
                "GBP" => new CultureInfo("en-GB"),
                "JPY" => new CultureInfo("ja-JP"),
                "CAD" => new CultureInfo("en-CA"),
                "AUD" => new CultureInfo("en-AU"),
                _ => CultureInfo.CurrentCulture
            };
        }

        public static bool IsValidCurrencyCode(string currencyCode)
        {
            try
            {
                var culture = GetCultureInfo(currencyCode);
                return !string.IsNullOrEmpty(culture.NumberFormat.CurrencySymbol);
            }
            catch
            {
                return false;
            }
        }

        public static decimal RoundToCurrency(decimal amount, string currencyCode = "USD")
        {
            var culture = GetCultureInfo(currencyCode);
            var decimals = culture.NumberFormat.CurrencyDecimalDigits;
            return Math.Round(amount, decimals, MidpointRounding.AwayFromZero);
        }
    }
} 
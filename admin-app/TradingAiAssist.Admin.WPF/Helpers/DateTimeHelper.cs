using System;

namespace TradingAiAssist.Admin.WPF.Helpers
{
    public static class DateTimeHelper
    {
        public static string GetRelativeTime(DateTime dateTime)
        {
            var timeSpan = DateTime.Now - dateTime;

            if (timeSpan.TotalDays >= 1)
            {
                var days = (int)timeSpan.TotalDays;
                return days == 1 ? "1 day ago" : $"{days} days ago";
            }
            else if (timeSpan.TotalHours >= 1)
            {
                var hours = (int)timeSpan.TotalHours;
                return hours == 1 ? "1 hour ago" : $"{hours} hours ago";
            }
            else if (timeSpan.TotalMinutes >= 1)
            {
                var minutes = (int)timeSpan.TotalMinutes;
                return minutes == 1 ? "1 minute ago" : $"{minutes} minutes ago";
            }
            else
            {
                return "Just now";
            }
        }

        public static string GetFormattedDateTime(DateTime dateTime, string format = "g")
        {
            return dateTime.ToString(format);
        }

        public static string GetShortDate(DateTime dateTime)
        {
            return dateTime.ToString("MM/dd/yyyy");
        }

        public static string GetShortTime(DateTime dateTime)
        {
            return dateTime.ToString("HH:mm");
        }

        public static string GetFullDateTime(DateTime dateTime)
        {
            return dateTime.ToString("MM/dd/yyyy HH:mm:ss");
        }

        public static string GetDuration(TimeSpan timeSpan)
        {
            if (timeSpan.TotalDays >= 1)
            {
                return $"{(int)timeSpan.TotalDays}d {timeSpan.Hours}h {timeSpan.Minutes}m";
            }
            else if (timeSpan.TotalHours >= 1)
            {
                return $"{timeSpan.Hours}h {timeSpan.Minutes}m {timeSpan.Seconds}s";
            }
            else if (timeSpan.TotalMinutes >= 1)
            {
                return $"{timeSpan.Minutes}m {timeSpan.Seconds}s";
            }
            else
            {
                return $"{timeSpan.Seconds}s";
            }
        }

        public static bool IsToday(DateTime dateTime)
        {
            return dateTime.Date == DateTime.Today;
        }

        public static bool IsYesterday(DateTime dateTime)
        {
            return dateTime.Date == DateTime.Today.AddDays(-1);
        }

        public static bool IsThisWeek(DateTime dateTime)
        {
            var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(6);
            return dateTime.Date >= startOfWeek && dateTime.Date <= endOfWeek;
        }

        public static bool IsThisMonth(DateTime dateTime)
        {
            return dateTime.Year == DateTime.Now.Year && dateTime.Month == DateTime.Now.Month;
        }
    }
} 
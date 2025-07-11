namespace TradingAiAssist.Admin.Core.Models
{
    public class Notification
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public NotificationSeverity Severity { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public bool IsRead { get; set; }
        public string? UserId { get; set; }
        public string? Source { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
        public List<NotificationAction> Actions { get; set; } = new();
    }

    public class NotificationAction
    {
        public string Label { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? Url { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new();
    }

    public enum NotificationType
    {
        System,
        User,
        Security,
        Performance,
        Budget,
        Compliance,
        Maintenance
    }

    public enum NotificationSeverity
    {
        Info,
        Warning,
        Error,
        Critical
    }
} 
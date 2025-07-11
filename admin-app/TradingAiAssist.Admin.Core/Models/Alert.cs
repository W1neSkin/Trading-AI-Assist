namespace TradingAiAssist.Admin.Core.Models
{
    public class Alert
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public AlertSeverity Severity { get; set; } = AlertSeverity.Info;
        public AlertStatus Status { get; set; } = AlertStatus.Active;
        public string ServiceName { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? AcknowledgedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? AcknowledgedBy { get; set; }
        public string? ResolvedBy { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    public enum AlertSeverity
    {
        Info,
        Warning,
        Error,
        Critical
    }

    public enum AlertStatus
    {
        Active,
        Acknowledged,
        Resolved,
        Suppressed
    }
} 
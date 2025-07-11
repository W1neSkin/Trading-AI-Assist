namespace TradingAiAssist.Admin.Core.Models
{
    public class SystemHealthStatus
    {
        public HealthStatus OverallStatus { get; set; } = HealthStatus.Unknown;
        public DateTime LastChecked { get; set; } = DateTime.UtcNow;
        public List<ServiceStatus> Services { get; set; } = new List<ServiceStatus>();
        public SystemResourceUsage ResourceUsage { get; set; } = new SystemResourceUsage();
        public List<Alert> ActiveAlerts { get; set; } = new List<Alert>();
        public string? Message { get; set; }
    }

    public enum HealthStatus
    {
        Unknown,
        Healthy,
        Warning,
        Critical,
        Offline
    }
} 
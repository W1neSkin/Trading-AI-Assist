namespace TradingAiAssist.Admin.Core.Models
{
    public class ServiceStatus
    {
        public string Name { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public HealthStatus Status { get; set; } = HealthStatus.Unknown;
        public DateTime LastChecked { get; set; } = DateTime.UtcNow;
        public DateTime? LastHealthy { get; set; }
        public string? Endpoint { get; set; }
        public double ResponseTime { get; set; } = 0;
        public string? Version { get; set; }
        public string? Message { get; set; }
        public Dictionary<string, object> Metrics { get; set; } = new Dictionary<string, object>();
    }
} 
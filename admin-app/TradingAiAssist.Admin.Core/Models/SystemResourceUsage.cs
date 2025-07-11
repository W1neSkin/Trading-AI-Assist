namespace TradingAiAssist.Admin.Core.Models
{
    public class SystemResourceUsage
    {
        public double CpuUsage { get; set; } = 0;
        public double MemoryUsage { get; set; } = 0;
        public double DiskUsage { get; set; } = 0;
        public double NetworkUsage { get; set; } = 0;
        public long TotalMemory { get; set; } = 0;
        public long AvailableMemory { get; set; } = 0;
        public long TotalDiskSpace { get; set; } = 0;
        public long AvailableDiskSpace { get; set; } = 0;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Dictionary<string, double> CustomMetrics { get; set; } = new Dictionary<string, double>();
    }
} 
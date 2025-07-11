namespace TradingAiAssist.Admin.Core.Models
{
    public class PerformanceMetric
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string ServiceName { get; set; } = "";
        public double Value { get; set; } = 0;
        public string Unit { get; set; } = "";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public MetricType Type { get; set; } = MetricType.Counter;
        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();
    }

    public enum MetricType
    {
        Counter,
        Gauge,
        Histogram,
        Summary
    }
} 
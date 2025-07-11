namespace TradingAiAssist.Admin.Core.Models
{
    public class AiUsageReport
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        
        public int TotalRequests { get; set; }
        public decimal TotalCost { get; set; }
        
        public Dictionary<string, decimal> CostBreakdown { get; set; } = new();
        public Dictionary<string, int> RequestsByModel { get; set; } = new();
        public List<CostTrend> CostTrends { get; set; } = new();
        
        public AiUsageMetrics CurrentHour { get; set; } = new();
        public AiUsageMetrics CurrentDay { get; set; } = new();
        public AiUsageMetrics CurrentMonth { get; set; } = new();
        
        public List<AiRequest> RecentRequests { get; set; } = new();
        public List<ModelPerformance> ModelPerformance { get; set; } = new();
    }

    public class AiUsageMetrics
    {
        public int Requests { get; set; }
        public decimal Cost { get; set; }
        public double AverageResponseTime { get; set; }
        public int ErrorCount { get; set; }
        public double SuccessRate { get; set; }
    }

    public class CostTrend
    {
        public DateTime Date { get; set; }
        public decimal Cost { get; set; }
        public int Requests { get; set; }
        public string Model { get; set; } = string.Empty;
    }

    public class AiRequest
    {
        public string Id { get; set; } = "";
        public string UserId { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty; // Ollama or OpenRoute
        public string QueryType { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public double ResponseTime { get; set; }
        public decimal Cost { get; set; }
        public bool IsSuccessful { get; set; }
        public string? ErrorMessage { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class ModelPerformance
    {
        public string Model { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public int TotalRequests { get; set; }
        public int SuccessfulRequests { get; set; }
        public double SuccessRate { get; set; }
        public double AverageResponseTime { get; set; }
        public decimal TotalCost { get; set; }
        public decimal AverageCostPerRequest { get; set; }
        public List<PerformanceMetric> HourlyMetrics { get; set; } = new();
    }

    public class PerformanceMetric
    {
        public DateTime Hour { get; set; }
        public int Requests { get; set; }
        public decimal Cost { get; set; }
        public double AverageResponseTime { get; set; }
        public int Errors { get; set; }
    }

    public class AiBudget
    {
        public decimal DailyLimit { get; set; }
        public decimal MonthlyLimit { get; set; }
        public decimal CurrentDaySpent { get; set; }
        public decimal CurrentMonthSpent { get; set; }
        public bool IsOverBudget { get; set; }
        public List<BudgetAlert> Alerts { get; set; } = new();
    }

    public class BudgetAlert
    {
        public DateTime Timestamp { get; set; }
        public string Message { get; set; } = string.Empty;
        public AlertSeverity Severity { get; set; }
        public bool IsAcknowledged { get; set; }
    }

    public enum AlertSeverity
    {
        Info,
        Warning,
        Critical
    }
} 
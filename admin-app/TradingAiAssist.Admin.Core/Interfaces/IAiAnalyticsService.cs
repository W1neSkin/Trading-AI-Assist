using TradingAiAssist.Admin.Core.Models;

namespace TradingAiAssist.Admin.Core.Interfaces
{
    public interface IAiAnalyticsService
    {
        Task<AiUsageReport> GetUsageReportAsync(DateTime from, DateTime to);
        Task<AiUsageMetrics> GetCurrentMetricsAsync();
        Task<List<AiRequest>> GetRecentRequestsAsync(int count = 100);
        Task<List<ModelPerformance>> GetModelPerformanceAsync();
        Task<AiBudget> GetBudgetInfoAsync();
        Task<List<CostTrend>> GetCostTrendsAsync(DateTime from, DateTime to);
        Task<Dictionary<string, decimal>> GetCostBreakdownAsync(DateTime from, DateTime to);
        Task<Dictionary<string, int>> GetRequestsByModelAsync(DateTime from, DateTime to);
        Task<double> GetAverageResponseTimeAsync(string model);
        Task<double> GetSuccessRateAsync(string model);
        Task<decimal> GetTotalCostAsync(DateTime from, DateTime to);
        Task<int> GetTotalRequestsAsync(DateTime from, DateTime to);
        Task<List<BudgetAlert>> GetBudgetAlertsAsync();
        Task<bool> SetBudgetLimitsAsync(decimal dailyLimit, decimal monthlyLimit);
        Task<bool> AcknowledgeAlertAsync(Guid alertId);
    }
} 
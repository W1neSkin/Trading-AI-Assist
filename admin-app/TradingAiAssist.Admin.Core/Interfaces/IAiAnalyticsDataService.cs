using TradingAiAssist.Admin.Core.Models;

namespace TradingAiAssist.Admin.Core.Interfaces
{
    public interface IAiAnalyticsDataService
    {
        Task<AiUsageReport> GetUsageReportAsync(DateTime startDate, DateTime endDate, string accessToken);
        Task<List<AiUsageReport>> GetDailyUsageAsync(DateTime startDate, DateTime endDate, string accessToken);
        Task<List<AiUsageReport>> GetUsageByUserAsync(string userId, DateTime startDate, DateTime endDate, string accessToken);
        Task<List<AiUsageReport>> GetUsageByServiceAsync(string serviceName, DateTime startDate, DateTime endDate, string accessToken);
        Task<decimal> GetTotalCostAsync(DateTime startDate, DateTime endDate, string accessToken);
        Task<decimal> GetCostByUserAsync(string userId, DateTime startDate, DateTime endDate, string accessToken);
        Task<decimal> GetCostByServiceAsync(string serviceName, DateTime startDate, DateTime endDate, string accessToken);
        Task<List<string>> GetTopUsersByCostAsync(DateTime startDate, DateTime endDate, int topCount, string accessToken);
        Task<List<string>> GetTopServicesByCostAsync(DateTime startDate, DateTime endDate, int topCount, string accessToken);
        Task<bool> SetCostAlertAsync(string userId, decimal threshold, string accessToken);
        Task<bool> RemoveCostAlertAsync(string userId, string accessToken);
    }
} 
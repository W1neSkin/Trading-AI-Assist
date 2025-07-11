using TradingAiAssist.Admin.Core.Models;

namespace TradingAiAssist.Admin.Core.Interfaces
{
    public interface ISystemHealthDataService
    {
        Task<SystemHealthStatus> GetSystemHealthAsync(string accessToken);
        Task<List<ServiceStatus>> GetServiceStatusAsync(string accessToken);
        Task<ServiceStatus?> GetServiceStatusAsync(string serviceName, string accessToken);
        Task<List<PerformanceMetric>> GetPerformanceMetricsAsync(DateTime startTime, DateTime endTime, string accessToken);
        Task<List<PerformanceMetric>> GetPerformanceMetricsAsync(string serviceName, DateTime startTime, DateTime endTime, string accessToken);
        Task<List<Alert>> GetActiveAlertsAsync(string accessToken);
        Task<bool> AcknowledgeAlertAsync(string alertId, string accessToken);
        Task<bool> ResolveAlertAsync(string alertId, string accessToken);
        Task<bool> RestartServiceAsync(string serviceName, string accessToken);
        Task<SystemResourceUsage> GetResourceUsageAsync(string accessToken);
    }
} 
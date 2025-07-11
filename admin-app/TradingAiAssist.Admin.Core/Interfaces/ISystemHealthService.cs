using TradingAiAssist.Admin.Core.Models;

namespace TradingAiAssist.Admin.Core.Interfaces
{
    public interface ISystemHealthService
    {
        Task<SystemHealthStatus> GetSystemHealthAsync();
        Task<List<ServiceStatus>> GetServiceStatusAsync();
        Task<ServiceStatus?> GetServiceStatusAsync(string serviceName);
        Task<List<PerformanceMetric>> GetPerformanceMetricsAsync(DateTime startTime, DateTime endTime);
        Task<List<PerformanceMetric>> GetPerformanceMetricsAsync(string serviceName, DateTime startTime, DateTime endTime);
        Task<List<Alert>> GetActiveAlertsAsync();
        Task<bool> AcknowledgeAlertAsync(string alertId);
        Task<bool> ResolveAlertAsync(string alertId);
        Task<bool> RestartServiceAsync(string serviceName);
        Task<SystemResourceUsage> GetResourceUsageAsync();
    }
} 
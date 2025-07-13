using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using TradingAiAssist.Admin.Core.Interfaces;
using TradingAiAssist.Admin.Core.Models;

namespace TradingAiAssist.Admin.Data.Services
{
    public class SystemHealthDataService : ISystemHealthDataService
    {
        private readonly ILogger<SystemHealthDataService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ApiOptions _apiOptions;

        public SystemHealthDataService(
            ILogger<SystemHealthDataService> logger,
            IHttpClientFactory httpClientFactory,
            IOptions<ApiOptions> apiOptions)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _apiOptions = apiOptions.Value;
        }

        private HttpClient CreateClient()
        {
            return _httpClientFactory.CreateClient("TradingAiAssistApi");
        }

        public async Task<SystemHealthStatus> GetSystemHealthAsync(string accessToken)
        {
            try
            {
                _logger.LogInformation("Retrieving system health status");
                
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiOptions.BaseUrl}/api/system-health");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await CreateClient().SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var healthStatus = JsonSerializer.Deserialize<SystemHealthStatus>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Retrieved system health status: {Status}", healthStatus?.OverallStatus);
                return healthStatus ?? new SystemHealthStatus { OverallStatus = HealthStatus.Unknown };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve system health status");
                return new SystemHealthStatus { OverallStatus = HealthStatus.Unknown };
            }
        }

        public async Task<List<ServiceStatus>> GetServiceStatusAsync(string accessToken)
        {
            try
            {
                _logger.LogInformation("Retrieving service status");
                
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiOptions.BaseUrl}/api/system-health/services");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await CreateClient().SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var services = JsonSerializer.Deserialize<List<ServiceStatus>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Retrieved status for {Count} services", services?.Count ?? 0);
                return services ?? new List<ServiceStatus>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve service status");
                return new List<ServiceStatus>();
            }
        }

        public async Task<ServiceStatus?> GetServiceStatusAsync(string serviceName, string accessToken)
        {
            try
            {
                _logger.LogInformation("Retrieving status for service: {ServiceName}", serviceName);
                
                var encodedServiceName = Uri.EscapeDataString(serviceName);
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiOptions.BaseUrl}/api/system-health/services/{encodedServiceName}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await CreateClient().SendAsync(request);
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Service not found: {ServiceName}", serviceName);
                    return null;
                }
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var serviceStatus = JsonSerializer.Deserialize<ServiceStatus>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Retrieved status for service {ServiceName}: {Status}", serviceName, serviceStatus?.Status);
                return serviceStatus;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve status for service: {ServiceName}", serviceName);
                return null;
            }
        }

        public async Task<List<PerformanceMetric>> GetPerformanceMetricsAsync(DateTime startTime, DateTime endTime, string accessToken)
        {
            try
            {
                _logger.LogInformation("Retrieving performance metrics from {StartTime} to {EndTime}", startTime, endTime);
                
                var startTimeStr = startTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
                var endTimeStr = endTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiOptions.BaseUrl}/api/system-health/performance-metrics?startTime={startTimeStr}&endTime={endTimeStr}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await CreateClient().SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var metrics = JsonSerializer.Deserialize<List<PerformanceMetric>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Retrieved {Count} performance metrics", metrics?.Count ?? 0);
                return metrics ?? new List<PerformanceMetric>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve performance metrics");
                return new List<PerformanceMetric>();
            }
        }

        public async Task<List<PerformanceMetric>> GetPerformanceMetricsAsync(string serviceName, DateTime startTime, DateTime endTime, string accessToken)
        {
            try
            {
                _logger.LogInformation("Retrieving performance metrics for service {ServiceName} from {StartTime} to {EndTime}", serviceName, startTime, endTime);
                
                var startTimeStr = startTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
                var endTimeStr = endTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
                var encodedServiceName = Uri.EscapeDataString(serviceName);
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiOptions.BaseUrl}/api/system-health/performance-metrics/{encodedServiceName}?startTime={startTimeStr}&endTime={endTimeStr}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await CreateClient().SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var metrics = JsonSerializer.Deserialize<List<PerformanceMetric>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Retrieved {Count} performance metrics for service {ServiceName}", metrics?.Count ?? 0, serviceName);
                return metrics ?? new List<PerformanceMetric>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve performance metrics for service: {ServiceName}", serviceName);
                return new List<PerformanceMetric>();
            }
        }

        public async Task<List<Alert>> GetActiveAlertsAsync(string accessToken)
        {
            try
            {
                _logger.LogInformation("Retrieving active alerts");
                
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiOptions.BaseUrl}/api/system-health/alerts/active");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await CreateClient().SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var alerts = JsonSerializer.Deserialize<List<Alert>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Retrieved {Count} active alerts", alerts?.Count ?? 0);
                return alerts ?? new List<Alert>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve active alerts");
                return new List<Alert>();
            }
        }

        public async Task<bool> AcknowledgeAlertAsync(string alertId, string accessToken)
        {
            try
            {
                _logger.LogInformation("Acknowledging alert: {AlertId}", alertId);
                
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiOptions.BaseUrl}/api/system-health/alerts/{alertId}/acknowledge");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await CreateClient().SendAsync(request);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Successfully acknowledged alert: {AlertId}", alertId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to acknowledge alert: {AlertId}", alertId);
                return false;
            }
        }

        public async Task<bool> ResolveAlertAsync(string alertId, string accessToken)
        {
            try
            {
                _logger.LogInformation("Resolving alert: {AlertId}", alertId);
                
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiOptions.BaseUrl}/api/system-health/alerts/{alertId}/resolve");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await CreateClient().SendAsync(request);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Successfully resolved alert: {AlertId}", alertId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resolve alert: {AlertId}", alertId);
                return false;
            }
        }

        public async Task<bool> RestartServiceAsync(string serviceName, string accessToken)
        {
            try
            {
                _logger.LogInformation("Restarting service: {ServiceName}", serviceName);
                
                var encodedServiceName = Uri.EscapeDataString(serviceName);
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiOptions.BaseUrl}/api/system-health/services/{encodedServiceName}/restart");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await CreateClient().SendAsync(request);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Successfully initiated restart for service: {ServiceName}", serviceName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to restart service: {ServiceName}", serviceName);
                return false;
            }
        }

        public async Task<SystemResourceUsage> GetResourceUsageAsync(string accessToken)
        {
            try
            {
                _logger.LogInformation("Retrieving system resource usage");
                
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiOptions.BaseUrl}/api/system-health/resources");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await CreateClient().SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var resourceUsage = JsonSerializer.Deserialize<SystemResourceUsage>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Retrieved system resource usage - CPU: {CpuUsage}%, Memory: {MemoryUsage}%, Disk: {DiskUsage}%", 
                    resourceUsage?.CpuUsage ?? 0, resourceUsage?.MemoryUsage ?? 0, resourceUsage?.DiskUsage ?? 0);
                return resourceUsage ?? new SystemResourceUsage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve system resource usage");
                return new SystemResourceUsage();
            }
        }
    }
} 
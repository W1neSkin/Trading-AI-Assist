using Microsoft.Extensions.Logging;
using TradingAiAssist.Admin.Core.Interfaces;
using TradingAiAssist.Admin.Core.Models;

namespace TradingAiAssist.Admin.Services.Services
{
    public class SystemHealthService : ISystemHealthService
    {
        private readonly ILogger<SystemHealthService> _logger;
        private readonly IAuthenticationService _authService;
        private readonly ISystemHealthDataService _healthDataService;

        public SystemHealthService(
            ILogger<SystemHealthService> logger,
            IAuthenticationService authService,
            ISystemHealthDataService healthDataService)
        {
            _logger = logger;
            _authService = authService;
            _healthDataService = healthDataService;
        }

        public async Task<SystemHealthStatus> GetSystemHealthAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving system health status");
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return new SystemHealthStatus { OverallStatus = HealthStatus.Unknown };
                }

                var healthStatus = await _healthDataService.GetSystemHealthAsync(authResult.AccessToken!);
                _logger.LogInformation("Retrieved system health status: {Status}", healthStatus.OverallStatus);
                
                return healthStatus;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve system health status");
                return new SystemHealthStatus { OverallStatus = HealthStatus.Unknown };
            }
        }

        public async Task<List<ServiceStatus>> GetServiceStatusAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving service status");
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return new List<ServiceStatus>();
                }

                var services = await _healthDataService.GetServiceStatusAsync(authResult.AccessToken!);
                _logger.LogInformation("Retrieved status for {Count} services", services.Count);
                
                return services;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve service status");
                return new List<ServiceStatus>();
            }
        }

        public async Task<ServiceStatus?> GetServiceStatusAsync(string serviceName)
        {
            try
            {
                _logger.LogInformation("Retrieving status for service: {ServiceName}", serviceName);
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return null;
                }

                var serviceStatus = await _healthDataService.GetServiceStatusAsync(serviceName, authResult.AccessToken!);
                if (serviceStatus != null)
                {
                    _logger.LogInformation("Retrieved status for service {ServiceName}: {Status}", serviceName, serviceStatus.Status);
                }
                else
                {
                    _logger.LogWarning("Service not found: {ServiceName}", serviceName);
                }
                
                return serviceStatus;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve status for service: {ServiceName}", serviceName);
                return null;
            }
        }

        public async Task<List<PerformanceMetric>> GetPerformanceMetricsAsync(DateTime startTime, DateTime endTime)
        {
            try
            {
                _logger.LogInformation("Retrieving performance metrics from {StartTime} to {EndTime}", startTime, endTime);
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return new List<PerformanceMetric>();
                }

                var metrics = await _healthDataService.GetPerformanceMetricsAsync(startTime, endTime, authResult.AccessToken!);
                _logger.LogInformation("Retrieved {Count} performance metrics", metrics.Count);
                
                return metrics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve performance metrics");
                return new List<PerformanceMetric>();
            }
        }

        public async Task<List<PerformanceMetric>> GetPerformanceMetricsAsync(string serviceName, DateTime startTime, DateTime endTime)
        {
            try
            {
                _logger.LogInformation("Retrieving performance metrics for service {ServiceName} from {StartTime} to {EndTime}", serviceName, startTime, endTime);
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return new List<PerformanceMetric>();
                }

                var metrics = await _healthDataService.GetPerformanceMetricsAsync(serviceName, startTime, endTime, authResult.AccessToken!);
                _logger.LogInformation("Retrieved {Count} performance metrics for service {ServiceName}", metrics.Count, serviceName);
                
                return metrics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve performance metrics for service: {ServiceName}", serviceName);
                return new List<PerformanceMetric>();
            }
        }

        public async Task<List<Alert>> GetActiveAlertsAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving active alerts");
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return new List<Alert>();
                }

                var alerts = await _healthDataService.GetActiveAlertsAsync(authResult.AccessToken!);
                _logger.LogInformation("Retrieved {Count} active alerts", alerts.Count);
                
                return alerts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve active alerts");
                return new List<Alert>();
            }
        }

        public async Task<bool> AcknowledgeAlertAsync(string alertId)
        {
            try
            {
                _logger.LogInformation("Acknowledging alert: {AlertId}", alertId);
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return false;
                }

                var success = await _healthDataService.AcknowledgeAlertAsync(alertId, authResult.AccessToken!);
                if (success)
                {
                    _logger.LogInformation("Successfully acknowledged alert: {AlertId}", alertId);
                }
                else
                {
                    _logger.LogWarning("Failed to acknowledge alert: {AlertId}", alertId);
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to acknowledge alert: {AlertId}", alertId);
                return false;
            }
        }

        public async Task<bool> ResolveAlertAsync(string alertId)
        {
            try
            {
                _logger.LogInformation("Resolving alert: {AlertId}", alertId);
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return false;
                }

                var success = await _healthDataService.ResolveAlertAsync(alertId, authResult.AccessToken!);
                if (success)
                {
                    _logger.LogInformation("Successfully resolved alert: {AlertId}", alertId);
                }
                else
                {
                    _logger.LogWarning("Failed to resolve alert: {AlertId}", alertId);
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resolve alert: {AlertId}", alertId);
                return false;
            }
        }

        public async Task<bool> RestartServiceAsync(string serviceName)
        {
            try
            {
                _logger.LogInformation("Restarting service: {ServiceName}", serviceName);
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return false;
                }

                var success = await _healthDataService.RestartServiceAsync(serviceName, authResult.AccessToken!);
                if (success)
                {
                    _logger.LogInformation("Successfully initiated restart for service: {ServiceName}", serviceName);
                }
                else
                {
                    _logger.LogWarning("Failed to restart service: {ServiceName}", serviceName);
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to restart service: {ServiceName}", serviceName);
                return false;
            }
        }

        public async Task<SystemResourceUsage> GetResourceUsageAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving system resource usage");
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return new SystemResourceUsage();
                }

                var resourceUsage = await _healthDataService.GetResourceUsageAsync(authResult.AccessToken!);
                _logger.LogInformation("Retrieved system resource usage - CPU: {CpuUsage}%, Memory: {MemoryUsage}%, Disk: {DiskUsage}%", 
                    resourceUsage.CpuUsage, resourceUsage.MemoryUsage, resourceUsage.DiskUsage);
                
                return resourceUsage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve system resource usage");
                return new SystemResourceUsage();
            }
        }
    }
} 
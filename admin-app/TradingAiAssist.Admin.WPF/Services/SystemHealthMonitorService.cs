using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using TradingAiAssist.Admin.Core.Interfaces;
using TradingAiAssist.Admin.Core.Models;

namespace TradingAiAssist.Admin.WPF.Services
{
    /// <summary>
    /// Background service for monitoring system health and providing real-time updates
    /// </summary>
    public class SystemHealthMonitorService : BackgroundService
    {
        private readonly ILogger<SystemHealthMonitorService> _logger;
        private readonly ISystemHealthService _systemHealthService;
        private readonly INotificationService _notificationService;
        private readonly TimeSpan _monitoringInterval = TimeSpan.FromSeconds(30); // Monitor every 30 seconds

        public SystemHealthMonitorService(
            ILogger<SystemHealthMonitorService> logger,
            ISystemHealthService systemHealthService,
            INotificationService notificationService)
        {
            _logger = logger;
            _systemHealthService = systemHealthService;
            _notificationService = notificationService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("System Health Monitor Service started");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await MonitorSystemHealthAsync();
                    await Task.Delay(_monitoringInterval, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("System Health Monitor Service stopped");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in System Health Monitor Service");
            }
        }

        private async Task MonitorSystemHealthAsync()
        {
            try
            {
                _logger.LogDebug("Starting system health monitoring cycle");

                // Get current system health status
                var healthStatus = await _systemHealthService.GetSystemHealthStatusAsync();
                
                if (healthStatus != null)
                {
                    // Check for critical issues
                    await CheckForCriticalIssuesAsync(healthStatus);
                    
                    // Update performance metrics
                    await UpdatePerformanceMetricsAsync(healthStatus);
                    
                    // Log health status for debugging
                    _logger.LogDebug("System health status: {Status}, Services: {ServiceCount}", 
                        healthStatus.OverallStatus, healthStatus.Services?.Count ?? 0);
                }
                else
                {
                    _logger.LogWarning("Failed to retrieve system health status");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during system health monitoring");
            }
        }

        private async Task CheckForCriticalIssuesAsync(SystemHealthStatus healthStatus)
        {
            try
            {
                // Check overall system status
                if (healthStatus.OverallStatus == ServiceStatus.Down)
                {
                    await _notificationService.SendAlertAsync(
                        "System Critical", 
                        "System is currently down. Immediate attention required.",
                        AlertSeverity.Critical);
                    
                    _logger.LogError("Critical system issue detected: System is down");
                }

                // Check individual services
                if (healthStatus.Services != null)
                {
                    foreach (var service in healthStatus.Services)
                    {
                        if (service.Status == ServiceStatus.Down)
                        {
                            await _notificationService.SendAlertAsync(
                                "Service Down", 
                                $"Service '{service.Name}' is currently down.",
                                AlertSeverity.High);
                            
                            _logger.LogWarning("Service down detected: {ServiceName}", service.Name);
                        }
                        else if (service.Status == ServiceStatus.Degraded)
                        {
                            await _notificationService.SendAlertAsync(
                                "Service Degraded", 
                                $"Service '{service.Name}' is experiencing issues.",
                                AlertSeverity.Medium);
                            
                            _logger.LogWarning("Service degraded: {ServiceName}", service.Name);
                        }
                    }
                }

                // Check resource usage
                if (healthStatus.ResourceUsage != null)
                {
                    var resources = healthStatus.ResourceUsage;
                    
                    if (resources.CpuUsage > 90)
                    {
                        await _notificationService.SendAlertAsync(
                            "High CPU Usage", 
                            $"CPU usage is at {resources.CpuUsage}%",
                            AlertSeverity.High);
                    }
                    
                    if (resources.MemoryUsage > 90)
                    {
                        await _notificationService.SendAlertAsync(
                            "High Memory Usage", 
                            $"Memory usage is at {resources.MemoryUsage}%",
                            AlertSeverity.High);
                    }
                    
                    if (resources.DiskUsage > 95)
                    {
                        await _notificationService.SendAlertAsync(
                            "High Disk Usage", 
                            $"Disk usage is at {resources.DiskUsage}%",
                            AlertSeverity.Critical);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for critical issues");
            }
        }

        private async Task UpdatePerformanceMetricsAsync(SystemHealthStatus healthStatus)
        {
            try
            {
                // Update performance metrics in the service
                if (healthStatus.PerformanceMetrics != null)
                {
                    await _systemHealthService.UpdatePerformanceMetricsAsync(healthStatus.PerformanceMetrics);
                }

                // Log performance data for monitoring
                if (healthStatus.ResourceUsage != null)
                {
                    _logger.LogDebug("Performance metrics - CPU: {Cpu}%, Memory: {Memory}%, Disk: {Disk}%",
                        healthStatus.ResourceUsage.CpuUsage,
                        healthStatus.ResourceUsage.MemoryUsage,
                        healthStatus.ResourceUsage.DiskUsage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating performance metrics");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping System Health Monitor Service");
            await base.StopAsync(cancellationToken);
        }
    }
} 
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
    /// Background service for monitoring AI usage and costs in real-time
    /// </summary>
    public class AiUsageMonitorService : BackgroundService
    {
        private readonly ILogger<AiUsageMonitorService> _logger;
        private readonly IAiAnalyticsService _aiAnalyticsService;
        private readonly INotificationService _notificationService;
        private readonly TimeSpan _monitoringInterval = TimeSpan.FromMinutes(1); // Monitor every minute

        public AiUsageMonitorService(
            ILogger<AiUsageMonitorService> logger,
            IAiAnalyticsService aiAnalyticsService,
            INotificationService notificationService)
        {
            _logger = logger;
            _aiAnalyticsService = aiAnalyticsService;
            _notificationService = notificationService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AI Usage Monitor Service started");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await MonitorAiUsageAsync();
                    await Task.Delay(_monitoringInterval, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("AI Usage Monitor Service stopped");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AI Usage Monitor Service");
            }
        }

        private async Task MonitorAiUsageAsync()
        {
            try
            {
                _logger.LogDebug("Starting AI usage monitoring cycle");

                // Get current AI usage report
                var usageReport = await _aiAnalyticsService.GetCurrentUsageReportAsync();
                
                if (usageReport != null)
                {
                    // Check for cost thresholds
                    await CheckCostThresholdsAsync(usageReport);
                    
                    // Monitor usage patterns
                    await MonitorUsagePatternsAsync(usageReport);
                    
                    // Update analytics data
                    await UpdateAnalyticsDataAsync(usageReport);
                    
                    // Log usage data for monitoring
                    _logger.LogDebug("AI usage - Total Cost: ${TotalCost}, Requests: {RequestCount}, Errors: {ErrorCount}",
                        usageReport.TotalCost,
                        usageReport.TotalRequests,
                        usageReport.ErrorCount);
                }
                else
                {
                    _logger.LogWarning("Failed to retrieve AI usage report");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during AI usage monitoring");
            }
        }

        private async Task CheckCostThresholdsAsync(AiUsageReport usageReport)
        {
            try
            {
                // Check daily cost threshold (example: $100)
                const decimal dailyCostThreshold = 100.0m;
                if (usageReport.DailyCost > dailyCostThreshold)
                {
                    await _notificationService.SendAlertAsync(
                        "High AI Costs", 
                        $"Daily AI costs have exceeded ${dailyCostThreshold}. Current cost: ${usageReport.DailyCost:F2}",
                        AlertSeverity.High);
                    
                    _logger.LogWarning("Daily AI cost threshold exceeded: ${Cost}", usageReport.DailyCost);
                }

                // Check monthly cost threshold (example: $2000)
                const decimal monthlyCostThreshold = 2000.0m;
                if (usageReport.MonthlyCost > monthlyCostThreshold)
                {
                    await _notificationService.SendAlertAsync(
                        "Monthly Cost Alert", 
                        $"Monthly AI costs have exceeded ${monthlyCostThreshold}. Current cost: ${usageReport.MonthlyCost:F2}",
                        AlertSeverity.Critical);
                    
                    _logger.LogWarning("Monthly AI cost threshold exceeded: ${Cost}", usageReport.MonthlyCost);
                }

                // Check hourly rate threshold
                const decimal hourlyRateThreshold = 50.0m;
                if (usageReport.HourlyCost > hourlyRateThreshold)
                {
                    await _notificationService.SendAlertAsync(
                        "High Hourly Usage", 
                        $"High AI usage detected. Hourly cost: ${usageReport.HourlyCost:F2}",
                        AlertSeverity.Medium);
                    
                    _logger.LogWarning("High hourly AI usage: ${Cost}/hour", usageReport.HourlyCost);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking cost thresholds");
            }
        }

        private async Task MonitorUsagePatternsAsync(AiUsageReport usageReport)
        {
            try
            {
                // Check for unusual error rates
                const double errorRateThreshold = 0.1; // 10% error rate
                if (usageReport.TotalRequests > 0)
                {
                    double errorRate = (double)usageReport.ErrorCount / usageReport.TotalRequests;
                    if (errorRate > errorRateThreshold)
                    {
                        await _notificationService.SendAlertAsync(
                            "High Error Rate", 
                            $"AI service error rate is {errorRate:P1}. This may indicate service issues.",
                            AlertSeverity.High);
                        
                        _logger.LogWarning("High AI error rate detected: {ErrorRate:P1}", errorRate);
                    }
                }

                // Check for unusual request volume
                const int requestVolumeThreshold = 1000; // requests per hour
                if (usageReport.HourlyRequests > requestVolumeThreshold)
                {
                    await _notificationService.SendAlertAsync(
                        "High Request Volume", 
                        $"Unusually high AI request volume: {usageReport.HourlyRequests} requests/hour",
                        AlertSeverity.Medium);
                    
                    _logger.LogWarning("High AI request volume: {Requests}/hour", usageReport.HourlyRequests);
                }

                // Check model performance
                if (usageReport.ModelPerformance != null)
                {
                    foreach (var model in usageReport.ModelPerformance)
                    {
                        if (model.AverageResponseTime > 5000) // 5 seconds
                        {
                            await _notificationService.SendAlertAsync(
                                "Slow Model Response", 
                                $"Model '{model.ModelName}' is responding slowly: {model.AverageResponseTime}ms average",
                                AlertSeverity.Medium);
                            
                            _logger.LogWarning("Slow model response: {Model} - {ResponseTime}ms", 
                                model.ModelName, model.AverageResponseTime);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error monitoring usage patterns");
            }
        }

        private async Task UpdateAnalyticsDataAsync(AiUsageReport usageReport)
        {
            try
            {
                // Update analytics data in the service
                await _aiAnalyticsService.UpdateUsageDataAsync(usageReport);

                // Log analytics data for monitoring
                _logger.LogDebug("Analytics updated - Models: {ModelCount}, Providers: {ProviderCount}",
                    usageReport.ModelPerformance?.Count ?? 0,
                    usageReport.ProviderCosts?.Count ?? 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating analytics data");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping AI Usage Monitor Service");
            await base.StopAsync(cancellationToken);
        }
    }
} 
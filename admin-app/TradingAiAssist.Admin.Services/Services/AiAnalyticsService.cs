using Microsoft.Extensions.Logging;
using TradingAiAssist.Admin.Core.Interfaces;
using TradingAiAssist.Admin.Core.Models;

namespace TradingAiAssist.Admin.Services.Services
{
    public class AiAnalyticsService : IAiAnalyticsService
    {
        private readonly ILogger<AiAnalyticsService> _logger;
        private readonly IAuthenticationService _authService;
        private readonly IAiAnalyticsDataService _aiDataService;

        public AiAnalyticsService(
            ILogger<AiAnalyticsService> logger,
            IAuthenticationService authService,
            IAiAnalyticsDataService aiDataService)
        {
            _logger = logger;
            _authService = authService;
            _aiDataService = aiDataService;
        }

        public async Task<AiUsageReport> GetUsageReportAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                _logger.LogInformation("Retrieving AI usage report from {StartDate} to {EndDate}", startDate, endDate);
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return new AiUsageReport();
                }

                var report = await _aiDataService.GetUsageReportAsync(startDate, endDate, authResult.AccessToken!);
                _logger.LogInformation("Retrieved AI usage report with {TotalRequests} requests and {TotalCost:C} cost", 
                    report.TotalRequests, report.TotalCost);
                
                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve AI usage report");
                return new AiUsageReport();
            }
        }

        public async Task<List<AiUsageReport>> GetDailyUsageAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                _logger.LogInformation("Retrieving daily AI usage from {StartDate} to {EndDate}", startDate, endDate);
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return new List<AiUsageReport>();
                }

                var dailyReports = await _aiDataService.GetDailyUsageAsync(startDate, endDate, authResult.AccessToken!);
                _logger.LogInformation("Retrieved {Count} daily usage reports", dailyReports.Count);
                
                return dailyReports;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve daily AI usage");
                return new List<AiUsageReport>();
            }
        }

        public async Task<List<AiUsageReport>> GetUsageByUserAsync(string userId, DateTime startDate, DateTime endDate)
        {
            try
            {
                _logger.LogInformation("Retrieving AI usage for user {UserId} from {StartDate} to {EndDate}", userId, startDate, endDate);
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return new List<AiUsageReport>();
                }

                var userReports = await _aiDataService.GetUsageByUserAsync(userId, startDate, endDate, authResult.AccessToken!);
                _logger.LogInformation("Retrieved {Count} usage reports for user {UserId}", userReports.Count, userId);
                
                return userReports;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve AI usage for user {UserId}", userId);
                return new List<AiUsageReport>();
            }
        }

        public async Task<List<AiUsageReport>> GetUsageByServiceAsync(string serviceName, DateTime startDate, DateTime endDate)
        {
            try
            {
                _logger.LogInformation("Retrieving AI usage for service {ServiceName} from {StartDate} to {EndDate}", serviceName, startDate, endDate);
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return new List<AiUsageReport>();
                }

                var serviceReports = await _aiDataService.GetUsageByServiceAsync(serviceName, startDate, endDate, authResult.AccessToken!);
                _logger.LogInformation("Retrieved {Count} usage reports for service {ServiceName}", serviceReports.Count, serviceName);
                
                return serviceReports;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve AI usage for service {ServiceName}", serviceName);
                return new List<AiUsageReport>();
            }
        }

        public async Task<decimal> GetTotalCostAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                _logger.LogInformation("Calculating total AI cost from {StartDate} to {EndDate}", startDate, endDate);
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return 0;
                }

                var totalCost = await _aiDataService.GetTotalCostAsync(startDate, endDate, authResult.AccessToken!);
                _logger.LogInformation("Total AI cost from {StartDate} to {EndDate}: {TotalCost:C}", startDate, endDate, totalCost);
                
                return totalCost;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to calculate total AI cost");
                return 0;
            }
        }

        public async Task<decimal> GetCostByUserAsync(string userId, DateTime startDate, DateTime endDate)
        {
            try
            {
                _logger.LogInformation("Calculating AI cost for user {UserId} from {StartDate} to {EndDate}", userId, startDate, endDate);
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return 0;
                }

                var userCost = await _aiDataService.GetCostByUserAsync(userId, startDate, endDate, authResult.AccessToken!);
                _logger.LogInformation("AI cost for user {UserId} from {StartDate} to {EndDate}: {UserCost:C}", userId, startDate, endDate, userCost);
                
                return userCost;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to calculate AI cost for user {UserId}", userId);
                return 0;
            }
        }

        public async Task<decimal> GetCostByServiceAsync(string serviceName, DateTime startDate, DateTime endDate)
        {
            try
            {
                _logger.LogInformation("Calculating AI cost for service {ServiceName} from {StartDate} to {EndDate}", serviceName, startDate, endDate);
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return 0;
                }

                var serviceCost = await _aiDataService.GetCostByServiceAsync(serviceName, startDate, endDate, authResult.AccessToken!);
                _logger.LogInformation("AI cost for service {ServiceName} from {StartDate} to {EndDate}: {ServiceCost:C}", serviceName, startDate, endDate, serviceCost);
                
                return serviceCost;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to calculate AI cost for service {ServiceName}", serviceName);
                return 0;
            }
        }

        public async Task<List<string>> GetTopUsersByCostAsync(DateTime startDate, DateTime endDate, int topCount = 10)
        {
            try
            {
                _logger.LogInformation("Retrieving top {TopCount} users by AI cost from {StartDate} to {EndDate}", topCount, startDate, endDate);
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return new List<string>();
                }

                var topUsers = await _aiDataService.GetTopUsersByCostAsync(startDate, endDate, topCount, authResult.AccessToken!);
                _logger.LogInformation("Retrieved top {Count} users by AI cost", topUsers.Count);
                
                return topUsers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve top users by AI cost");
                return new List<string>();
            }
        }

        public async Task<List<string>> GetTopServicesByCostAsync(DateTime startDate, DateTime endDate, int topCount = 10)
        {
            try
            {
                _logger.LogInformation("Retrieving top {TopCount} services by AI cost from {StartDate} to {EndDate}", topCount, startDate, endDate);
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return new List<string>();
                }

                var topServices = await _aiDataService.GetTopServicesByCostAsync(startDate, endDate, topCount, authResult.AccessToken!);
                _logger.LogInformation("Retrieved top {Count} services by AI cost", topServices.Count);
                
                return topServices;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve top services by AI cost");
                return new List<string>();
            }
        }

        public async Task<bool> SetCostAlertAsync(string userId, decimal threshold)
        {
            try
            {
                _logger.LogInformation("Setting cost alert for user {UserId} with threshold {Threshold:C}", userId, threshold);
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return false;
                }

                var success = await _aiDataService.SetCostAlertAsync(userId, threshold, authResult.AccessToken!);
                if (success)
                {
                    _logger.LogInformation("Successfully set cost alert for user {UserId}", userId);
                }
                else
                {
                    _logger.LogWarning("Failed to set cost alert for user {UserId}", userId);
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set cost alert for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> RemoveCostAlertAsync(string userId)
        {
            try
            {
                _logger.LogInformation("Removing cost alert for user {UserId}", userId);
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return false;
                }

                var success = await _aiDataService.RemoveCostAlertAsync(userId, authResult.AccessToken!);
                if (success)
                {
                    _logger.LogInformation("Successfully removed cost alert for user {UserId}", userId);
                }
                else
                {
                    _logger.LogWarning("Failed to remove cost alert for user {UserId}", userId);
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove cost alert for user {UserId}", userId);
                return false;
            }
        }
    }
} 
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using TradingAiAssist.Admin.Core.Interfaces;
using TradingAiAssist.Admin.Core.Models;

namespace TradingAiAssist.Admin.Data.Services
{
    public class AiAnalyticsDataService : IAiAnalyticsDataService
    {
        private readonly ILogger<AiAnalyticsDataService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ApiOptions _apiOptions;

        public AiAnalyticsDataService(
            ILogger<AiAnalyticsDataService> logger,
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

        public async Task<AiUsageReport> GetUsageReportAsync(DateTime startDate, DateTime endDate, string accessToken)
        {
            try
            {
                _logger.LogInformation("Retrieving AI usage report from {StartDate} to {EndDate}", startDate, endDate);
                
                var startDateStr = startDate.ToString("yyyy-MM-dd");
                var endDateStr = endDate.ToString("yyyy-MM-dd");
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiOptions.BaseUrl}/api/ai-analytics/usage-report?startDate={startDateStr}&endDate={endDateStr}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await CreateClient().SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var report = JsonSerializer.Deserialize<AiUsageReport>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Retrieved AI usage report with {TotalRequests} requests and {TotalCost:C} cost", 
                    report?.TotalRequests ?? 0, report?.TotalCost ?? 0);
                return report ?? new AiUsageReport();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve AI usage report");
                return new AiUsageReport();
            }
        }

        public async Task<List<AiUsageReport>> GetDailyUsageAsync(DateTime startDate, DateTime endDate, string accessToken)
        {
            try
            {
                _logger.LogInformation("Retrieving daily AI usage from {StartDate} to {EndDate}", startDate, endDate);
                
                var startDateStr = startDate.ToString("yyyy-MM-dd");
                var endDateStr = endDate.ToString("yyyy-MM-dd");
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiOptions.BaseUrl}/api/ai-analytics/daily-usage?startDate={startDateStr}&endDate={endDateStr}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await CreateClient().SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var dailyReports = JsonSerializer.Deserialize<List<AiUsageReport>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Retrieved {Count} daily usage reports", dailyReports?.Count ?? 0);
                return dailyReports ?? new List<AiUsageReport>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve daily AI usage");
                return new List<AiUsageReport>();
            }
        }

        public async Task<List<AiUsageReport>> GetUsageByUserAsync(string userId, DateTime startDate, DateTime endDate, string accessToken)
        {
            try
            {
                _logger.LogInformation("Retrieving AI usage for user {UserId} from {StartDate} to {EndDate}", userId, startDate, endDate);
                
                var startDateStr = startDate.ToString("yyyy-MM-dd");
                var endDateStr = endDate.ToString("yyyy-MM-dd");
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiOptions.BaseUrl}/api/ai-analytics/user-usage/{userId}?startDate={startDateStr}&endDate={endDateStr}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await CreateClient().SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var userReports = JsonSerializer.Deserialize<List<AiUsageReport>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Retrieved {Count} usage reports for user {UserId}", userReports?.Count ?? 0, userId);
                return userReports ?? new List<AiUsageReport>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve AI usage for user {UserId}", userId);
                return new List<AiUsageReport>();
            }
        }

        public async Task<List<AiUsageReport>> GetUsageByServiceAsync(string serviceName, DateTime startDate, DateTime endDate, string accessToken)
        {
            try
            {
                _logger.LogInformation("Retrieving AI usage for service {ServiceName} from {StartDate} to {EndDate}", serviceName, startDate, endDate);
                
                var startDateStr = startDate.ToString("yyyy-MM-dd");
                var endDateStr = endDate.ToString("yyyy-MM-dd");
                var encodedServiceName = Uri.EscapeDataString(serviceName);
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiOptions.BaseUrl}/api/ai-analytics/service-usage/{encodedServiceName}?startDate={startDateStr}&endDate={endDateStr}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await CreateClient().SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var serviceReports = JsonSerializer.Deserialize<List<AiUsageReport>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Retrieved {Count} usage reports for service {ServiceName}", serviceReports?.Count ?? 0, serviceName);
                return serviceReports ?? new List<AiUsageReport>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve AI usage for service {ServiceName}", serviceName);
                return new List<AiUsageReport>();
            }
        }

        public async Task<decimal> GetTotalCostAsync(DateTime startDate, DateTime endDate, string accessToken)
        {
            try
            {
                _logger.LogInformation("Calculating total AI cost from {StartDate} to {EndDate}", startDate, endDate);
                
                var startDateStr = startDate.ToString("yyyy-MM-dd");
                var endDateStr = endDate.ToString("yyyy-MM-dd");
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiOptions.BaseUrl}/api/ai-analytics/total-cost?startDate={startDateStr}&endDate={endDateStr}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await CreateClient().SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<CostResult>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Total AI cost from {StartDate} to {EndDate}: {TotalCost:C}", startDate, endDate, result?.TotalCost ?? 0);
                return result?.TotalCost ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to calculate total AI cost");
                return 0;
            }
        }

        public async Task<decimal> GetCostByUserAsync(string userId, DateTime startDate, DateTime endDate, string accessToken)
        {
            try
            {
                _logger.LogInformation("Calculating AI cost for user {UserId} from {StartDate} to {EndDate}", userId, startDate, endDate);
                
                var startDateStr = startDate.ToString("yyyy-MM-dd");
                var endDateStr = endDate.ToString("yyyy-MM-dd");
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiOptions.BaseUrl}/api/ai-analytics/user-cost/{userId}?startDate={startDateStr}&endDate={endDateStr}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await CreateClient().SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<CostResult>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("AI cost for user {UserId} from {StartDate} to {EndDate}: {UserCost:C}", userId, startDate, endDate, result?.TotalCost ?? 0);
                return result?.TotalCost ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to calculate AI cost for user {UserId}", userId);
                return 0;
            }
        }

        public async Task<decimal> GetCostByServiceAsync(string serviceName, DateTime startDate, DateTime endDate, string accessToken)
        {
            try
            {
                _logger.LogInformation("Calculating AI cost for service {ServiceName} from {StartDate} to {EndDate}", serviceName, startDate, endDate);
                
                var startDateStr = startDate.ToString("yyyy-MM-dd");
                var endDateStr = endDate.ToString("yyyy-MM-dd");
                var encodedServiceName = Uri.EscapeDataString(serviceName);
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiOptions.BaseUrl}/api/ai-analytics/service-cost/{encodedServiceName}?startDate={startDateStr}&endDate={endDateStr}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await CreateClient().SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<CostResult>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("AI cost for service {ServiceName} from {StartDate} to {EndDate}: {ServiceCost:C}", serviceName, startDate, endDate, result?.TotalCost ?? 0);
                return result?.TotalCost ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to calculate AI cost for service {ServiceName}", serviceName);
                return 0;
            }
        }

        public async Task<List<string>> GetTopUsersByCostAsync(DateTime startDate, DateTime endDate, int topCount, string accessToken)
        {
            try
            {
                _logger.LogInformation("Retrieving top {TopCount} users by AI cost from {StartDate} to {EndDate}", topCount, startDate, endDate);
                
                var startDateStr = startDate.ToString("yyyy-MM-dd");
                var endDateStr = endDate.ToString("yyyy-MM-dd");
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiOptions.BaseUrl}/api/ai-analytics/top-users?startDate={startDateStr}&endDate={endDateStr}&topCount={topCount}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await CreateClient().SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var topUsers = JsonSerializer.Deserialize<List<string>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Retrieved top {Count} users by AI cost", topUsers?.Count ?? 0);
                return topUsers ?? new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve top users by AI cost");
                return new List<string>();
            }
        }

        public async Task<List<string>> GetTopServicesByCostAsync(DateTime startDate, DateTime endDate, int topCount, string accessToken)
        {
            try
            {
                _logger.LogInformation("Retrieving top {TopCount} services by AI cost from {StartDate} to {EndDate}", topCount, startDate, endDate);
                
                var startDateStr = startDate.ToString("yyyy-MM-dd");
                var endDateStr = endDate.ToString("yyyy-MM-dd");
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiOptions.BaseUrl}/api/ai-analytics/top-services?startDate={startDateStr}&endDate={endDateStr}&topCount={topCount}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await CreateClient().SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var topServices = JsonSerializer.Deserialize<List<string>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Retrieved top {Count} services by AI cost", topServices?.Count ?? 0);
                return topServices ?? new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve top services by AI cost");
                return new List<string>();
            }
        }

        public async Task<bool> SetCostAlertAsync(string userId, decimal threshold, string accessToken)
        {
            try
            {
                _logger.LogInformation("Setting cost alert for user {UserId} with threshold {Threshold:C}", userId, threshold);
                
                var requestData = new { Threshold = threshold };
                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiOptions.BaseUrl}/api/ai-analytics/cost-alerts/{userId}")
                {
                    Content = content
                };
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await CreateClient().SendAsync(request);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Successfully set cost alert for user {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set cost alert for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> RemoveCostAlertAsync(string userId, string accessToken)
        {
            try
            {
                _logger.LogInformation("Removing cost alert for user {UserId}", userId);
                
                var request = new HttpRequestMessage(HttpMethod.Delete, $"{_apiOptions.BaseUrl}/api/ai-analytics/cost-alerts/{userId}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await CreateClient().SendAsync(request);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Successfully removed cost alert for user {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove cost alert for user {UserId}", userId);
                return false;
            }
        }

        private class CostResult
        {
            public decimal TotalCost { get; set; }
        }
    }
} 
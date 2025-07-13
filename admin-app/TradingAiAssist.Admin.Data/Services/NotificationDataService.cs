using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using TradingAiAssist.Admin.Core.Interfaces;
using TradingAiAssist.Admin.Core.Models;

namespace TradingAiAssist.Admin.Data.Services
{
    public class NotificationDataService : INotificationDataService
    {
        private readonly ILogger<NotificationDataService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ApiOptions _apiOptions;

        public NotificationDataService(
            ILogger<NotificationDataService> logger,
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

        public async Task<List<Notification>> GetNotificationsAsync(string accessToken)
        {
            try
            {
                _logger.LogInformation("Retrieving notifications from API");
                
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiOptions.BaseUrl}/api/notifications");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await CreateClient().SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var notifications = JsonSerializer.Deserialize<List<Notification>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Retrieved {Count} notifications from API", notifications?.Count ?? 0);
                return notifications ?? new List<Notification>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve notifications from API");
                return new List<Notification>();
            }
        }

        public async Task<List<Notification>> GetNotificationsByUserAsync(string userId, string accessToken)
        {
            try
            {
                _logger.LogInformation("Retrieving notifications for user: {UserId}", userId);
                
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiOptions.BaseUrl}/api/notifications/user/{userId}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await CreateClient().SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var notifications = JsonSerializer.Deserialize<List<Notification>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Retrieved {Count} notifications for user {UserId}", notifications?.Count ?? 0, userId);
                return notifications ?? new List<Notification>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve notifications for user: {UserId}", userId);
                return new List<Notification>();
            }
        }

        public async Task<List<Notification>> GetUnreadNotificationsAsync(string accessToken)
        {
            try
            {
                _logger.LogInformation("Retrieving unread notifications from API");
                
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiOptions.BaseUrl}/api/notifications/unread");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await CreateClient().SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var notifications = JsonSerializer.Deserialize<List<Notification>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Retrieved {Count} unread notifications from API", notifications?.Count ?? 0);
                return notifications ?? new List<Notification>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve unread notifications from API");
                return new List<Notification>();
            }
        }

        public async Task<Notification?> GetNotificationByIdAsync(string notificationId, string accessToken)
        {
            try
            {
                _logger.LogInformation("Retrieving notification: {NotificationId}", notificationId);
                
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiOptions.BaseUrl}/api/notifications/{notificationId}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await CreateClient().SendAsync(request);
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Notification not found: {NotificationId}", notificationId);
                    return null;
                }
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var notification = JsonSerializer.Deserialize<Notification>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Retrieved notification: {Title}", notification?.Title);
                return notification;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve notification: {NotificationId}", notificationId);
                return null;
            }
        }

        public async Task<bool> MarkAsReadAsync(string notificationId, string accessToken)
        {
            try
            {
                _logger.LogInformation("Marking notification as read: {NotificationId}", notificationId);
                
                var request = new HttpRequestMessage(HttpMethod.Put, $"{_apiOptions.BaseUrl}/api/notifications/{notificationId}/read");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await CreateClient().SendAsync(request);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Successfully marked notification as read: {NotificationId}", notificationId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to mark notification as read: {NotificationId}", notificationId);
                return false;
            }
        }

        public async Task<bool> MarkAllAsReadAsync(string accessToken)
        {
            try
            {
                _logger.LogInformation("Marking all notifications as read");
                
                var request = new HttpRequestMessage(HttpMethod.Put, $"{_apiOptions.BaseUrl}/api/notifications/read-all");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await CreateClient().SendAsync(request);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Successfully marked all notifications as read");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to mark all notifications as read");
                return false;
            }
        }

        public async Task<bool> DeleteNotificationAsync(string notificationId, string accessToken)
        {
            try
            {
                _logger.LogInformation("Deleting notification: {NotificationId}", notificationId);
                
                var request = new HttpRequestMessage(HttpMethod.Delete, $"{_apiOptions.BaseUrl}/api/notifications/{notificationId}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await CreateClient().SendAsync(request);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Successfully deleted notification: {NotificationId}", notificationId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete notification: {NotificationId}", notificationId);
                return false;
            }
        }

        public async Task<bool> SendNotificationAsync(Notification notification, string accessToken)
        {
            try
            {
                _logger.LogInformation("Sending notification: {Title}", notification.Title);
                
                var json = JsonSerializer.Serialize(notification);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiOptions.BaseUrl}/api/notifications")
                {
                    Content = content
                };
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await CreateClient().SendAsync(request);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Successfully sent notification: {Title}", notification.Title);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification: {Title}", notification.Title);
                return false;
            }
        }

        public async Task<bool> SendNotificationToAllUsersAsync(string title, string message, NotificationType type, string accessToken)
        {
            try
            {
                _logger.LogInformation("Sending notification to all users: {Title}", title);
                
                var requestData = new { Title = title, Message = message, Type = type };
                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiOptions.BaseUrl}/api/notifications/broadcast")
                {
                    Content = content
                };
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await CreateClient().SendAsync(request);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Successfully sent notification to all users: {Title}", title);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification to all users: {Title}", title);
                return false;
            }
        }

        public async Task<int> GetUnreadCountAsync(string accessToken)
        {
            try
            {
                _logger.LogInformation("Getting unread notification count");
                
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiOptions.BaseUrl}/api/notifications/unread-count");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await CreateClient().SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<CountResult>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Unread notification count: {Count}", result?.Count ?? 0);
                return result?.Count ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get unread notification count");
                return 0;
            }
        }

        public async Task<int> GetUnreadCountByUserAsync(string userId, string accessToken)
        {
            try
            {
                _logger.LogInformation("Getting unread notification count for user: {UserId}", userId);
                
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiOptions.BaseUrl}/api/notifications/unread-count/{userId}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await CreateClient().SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<CountResult>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Unread notification count for user {UserId}: {Count}", userId, result?.Count ?? 0);
                return result?.Count ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get unread notification count for user: {UserId}", userId);
                return 0;
            }
        }

        private class CountResult
        {
            public int Count { get; set; }
        }
    }
} 
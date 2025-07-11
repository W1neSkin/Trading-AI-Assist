using Microsoft.Extensions.Logging;
using TradingAiAssist.Admin.Core.Interfaces;
using TradingAiAssist.Admin.Core.Models;

namespace TradingAiAssist.Admin.Services.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;
        private readonly IAuthenticationService _authService;
        private readonly INotificationDataService _notificationDataService;

        public NotificationService(
            ILogger<NotificationService> logger,
            IAuthenticationService authService,
            INotificationDataService notificationDataService)
        {
            _logger = logger;
            _authService = authService;
            _notificationDataService = notificationDataService;
        }

        public async Task<List<Notification>> GetNotificationsAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving notifications");
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return new List<Notification>();
                }

                var notifications = await _notificationDataService.GetNotificationsAsync(authResult.AccessToken!);
                _logger.LogInformation("Retrieved {Count} notifications", notifications.Count);
                
                return notifications;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve notifications");
                return new List<Notification>();
            }
        }

        public async Task<List<Notification>> GetNotificationsByUserAsync(string userId)
        {
            try
            {
                _logger.LogInformation("Retrieving notifications for user: {UserId}", userId);
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return new List<Notification>();
                }

                var notifications = await _notificationDataService.GetNotificationsByUserAsync(userId, authResult.AccessToken!);
                _logger.LogInformation("Retrieved {Count} notifications for user {UserId}", notifications.Count, userId);
                
                return notifications;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve notifications for user: {UserId}", userId);
                return new List<Notification>();
            }
        }

        public async Task<List<Notification>> GetUnreadNotificationsAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving unread notifications");
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return new List<Notification>();
                }

                var notifications = await _notificationDataService.GetUnreadNotificationsAsync(authResult.AccessToken!);
                _logger.LogInformation("Retrieved {Count} unread notifications", notifications.Count);
                
                return notifications;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve unread notifications");
                return new List<Notification>();
            }
        }

        public async Task<Notification?> GetNotificationByIdAsync(string notificationId)
        {
            try
            {
                _logger.LogInformation("Retrieving notification: {NotificationId}", notificationId);
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return null;
                }

                var notification = await _notificationDataService.GetNotificationByIdAsync(notificationId, authResult.AccessToken!);
                if (notification != null)
                {
                    _logger.LogInformation("Retrieved notification: {Title}", notification.Title);
                }
                else
                {
                    _logger.LogWarning("Notification not found: {NotificationId}", notificationId);
                }
                
                return notification;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve notification: {NotificationId}", notificationId);
                return null;
            }
        }

        public async Task<bool> MarkAsReadAsync(string notificationId)
        {
            try
            {
                _logger.LogInformation("Marking notification as read: {NotificationId}", notificationId);
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return false;
                }

                var success = await _notificationDataService.MarkAsReadAsync(notificationId, authResult.AccessToken!);
                if (success)
                {
                    _logger.LogInformation("Successfully marked notification as read: {NotificationId}", notificationId);
                }
                else
                {
                    _logger.LogWarning("Failed to mark notification as read: {NotificationId}", notificationId);
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to mark notification as read: {NotificationId}", notificationId);
                return false;
            }
        }

        public async Task<bool> MarkAllAsReadAsync()
        {
            try
            {
                _logger.LogInformation("Marking all notifications as read");
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return false;
                }

                var success = await _notificationDataService.MarkAllAsReadAsync(authResult.AccessToken!);
                if (success)
                {
                    _logger.LogInformation("Successfully marked all notifications as read");
                }
                else
                {
                    _logger.LogWarning("Failed to mark all notifications as read");
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to mark all notifications as read");
                return false;
            }
        }

        public async Task<bool> DeleteNotificationAsync(string notificationId)
        {
            try
            {
                _logger.LogInformation("Deleting notification: {NotificationId}", notificationId);
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return false;
                }

                var success = await _notificationDataService.DeleteNotificationAsync(notificationId, authResult.AccessToken!);
                if (success)
                {
                    _logger.LogInformation("Successfully deleted notification: {NotificationId}", notificationId);
                }
                else
                {
                    _logger.LogWarning("Failed to delete notification: {NotificationId}", notificationId);
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete notification: {NotificationId}", notificationId);
                return false;
            }
        }

        public async Task<bool> SendNotificationAsync(Notification notification)
        {
            try
            {
                _logger.LogInformation("Sending notification: {Title}", notification.Title);
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return false;
                }

                var success = await _notificationDataService.SendNotificationAsync(notification, authResult.AccessToken!);
                if (success)
                {
                    _logger.LogInformation("Successfully sent notification: {Title}", notification.Title);
                }
                else
                {
                    _logger.LogWarning("Failed to send notification: {Title}", notification.Title);
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification: {Title}", notification.Title);
                return false;
            }
        }

        public async Task<bool> SendNotificationToUserAsync(string userId, string title, string message, NotificationType type = NotificationType.Info)
        {
            try
            {
                _logger.LogInformation("Sending notification to user {UserId}: {Title}", userId, title);
                
                var notification = new Notification
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = title,
                    Message = message,
                    Type = type,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };

                var success = await SendNotificationAsync(notification);
                if (success)
                {
                    _logger.LogInformation("Successfully sent notification to user {UserId}: {Title}", userId, title);
                }
                else
                {
                    _logger.LogWarning("Failed to send notification to user {UserId}: {Title}", userId, title);
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification to user {UserId}: {Title}", userId, title);
                return false;
            }
        }

        public async Task<bool> SendNotificationToAllUsersAsync(string title, string message, NotificationType type = NotificationType.Info)
        {
            try
            {
                _logger.LogInformation("Sending notification to all users: {Title}", title);
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return false;
                }

                var success = await _notificationDataService.SendNotificationToAllUsersAsync(title, message, type, authResult.AccessToken!);
                if (success)
                {
                    _logger.LogInformation("Successfully sent notification to all users: {Title}", title);
                }
                else
                {
                    _logger.LogWarning("Failed to send notification to all users: {Title}", title);
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification to all users: {Title}", title);
                return false;
            }
        }

        public async Task<int> GetUnreadCountAsync()
        {
            try
            {
                _logger.LogInformation("Getting unread notification count");
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return 0;
                }

                var count = await _notificationDataService.GetUnreadCountAsync(authResult.AccessToken!);
                _logger.LogInformation("Unread notification count: {Count}", count);
                
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get unread notification count");
                return 0;
            }
        }

        public async Task<int> GetUnreadCountByUserAsync(string userId)
        {
            try
            {
                _logger.LogInformation("Getting unread notification count for user: {UserId}", userId);
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return 0;
                }

                var count = await _notificationDataService.GetUnreadCountByUserAsync(userId, authResult.AccessToken!);
                _logger.LogInformation("Unread notification count for user {UserId}: {Count}", userId, count);
                
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get unread notification count for user: {UserId}", userId);
                return 0;
            }
        }
    }
} 
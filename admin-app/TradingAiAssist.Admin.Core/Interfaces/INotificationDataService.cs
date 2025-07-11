using TradingAiAssist.Admin.Core.Models;

namespace TradingAiAssist.Admin.Core.Interfaces
{
    public interface INotificationDataService
    {
        Task<List<Notification>> GetNotificationsAsync(string accessToken);
        Task<List<Notification>> GetNotificationsByUserAsync(string userId, string accessToken);
        Task<List<Notification>> GetUnreadNotificationsAsync(string accessToken);
        Task<Notification?> GetNotificationByIdAsync(string notificationId, string accessToken);
        Task<bool> MarkAsReadAsync(string notificationId, string accessToken);
        Task<bool> MarkAllAsReadAsync(string accessToken);
        Task<bool> DeleteNotificationAsync(string notificationId, string accessToken);
        Task<bool> SendNotificationAsync(Notification notification, string accessToken);
        Task<bool> SendNotificationToAllUsersAsync(string title, string message, NotificationType type, string accessToken);
        Task<int> GetUnreadCountAsync(string accessToken);
        Task<int> GetUnreadCountByUserAsync(string userId, string accessToken);
    }
} 
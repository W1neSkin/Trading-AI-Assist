using TradingAiAssist.Admin.Core.Models;

namespace TradingAiAssist.Admin.Core.Interfaces
{
    public interface INotificationService
    {
        Task<List<Notification>> GetNotificationsAsync();
        Task<List<Notification>> GetNotificationsByUserAsync(string userId);
        Task<List<Notification>> GetUnreadNotificationsAsync();
        Task<Notification?> GetNotificationByIdAsync(string notificationId);
        Task<bool> MarkAsReadAsync(string notificationId);
        Task<bool> MarkAllAsReadAsync();
        Task<bool> DeleteNotificationAsync(string notificationId);
        Task<bool> SendNotificationAsync(Notification notification);
        Task<bool> SendNotificationToUserAsync(string userId, string title, string message, NotificationType type = NotificationType.Info);
        Task<bool> SendNotificationToAllUsersAsync(string title, string message, NotificationType type = NotificationType.Info);
        Task<int> GetUnreadCountAsync();
        Task<int> GetUnreadCountByUserAsync(string userId);
    }
} 
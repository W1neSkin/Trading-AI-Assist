using TradingAiAssist.Admin.Core.Models;

namespace TradingAiAssist.Admin.Core.Interfaces
{
    public interface IUserManagementService
    {
        Task<List<UserProfile>> GetAllUsersAsync();
        Task<UserProfile?> GetUserByIdAsync(Guid userId);
        Task<UserProfile?> GetUserByEmailAsync(string email);
        Task<bool> CreateUserAsync(UserProfile user);
        Task<bool> UpdateUserAsync(UserProfile user);
        Task<bool> DeleteUserAsync(Guid userId);
        Task<bool> SuspendUserAsync(Guid userId);
        Task<bool> ActivateUserAsync(Guid userId);
        Task<bool> ChangeUserRoleAsync(Guid userId, UserRole newRole);
        Task<bool> ResetUserPasswordAsync(Guid userId);
        Task<List<UserProfile>> GetUsersByRoleAsync(UserRole role);
        Task<List<UserProfile>> GetUsersByStatusAsync(UserStatus status);
        Task<int> GetTotalUserCountAsync();
        Task<int> GetActiveUserCountAsync();
        Task<List<UserProfile>> SearchUsersAsync(string searchTerm);
        Task<bool> SendNotificationToUserAsync(Guid userId, string message);
        Task<List<UserProfile>> GetUsersByDepartmentAsync(string department);
        Task<Dictionary<UserRole, int>> GetUserCountByRoleAsync();
        Task<Dictionary<UserStatus, int>> GetUserCountByStatusAsync();
    }
} 
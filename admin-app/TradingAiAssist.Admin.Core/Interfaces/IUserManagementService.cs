using TradingAiAssist.Admin.Core.Models;

namespace TradingAiAssist.Admin.Core.Interfaces
{
    public interface IUserManagementService
    {
        Task<List<UserProfile>> GetAllUsersAsync();
        Task<UserProfile?> GetUserByIdAsync(string userId);
        Task<UserProfile?> GetUserByEmailAsync(string email);
        Task<bool> CreateUserAsync(UserProfile user);
        Task<bool> UpdateUserAsync(UserProfile user);
        Task<bool> DeleteUserAsync(string userId);
        Task<bool> SuspendUserAsync(string userId);
        Task<bool> ActivateUserAsync(string userId);
        Task<bool> ChangeUserRoleAsync(string userId, UserRole newRole);
        Task<bool> ResetUserPasswordAsync(string userId);
        Task<List<UserProfile>> GetUsersByRoleAsync(UserRole role);
        Task<List<UserProfile>> GetUsersByStatusAsync(UserStatus status);
        Task<int> GetTotalUserCountAsync();
        Task<int> GetActiveUserCountAsync();
        Task<List<UserProfile>> SearchUsersAsync(string searchTerm);
        Task<bool> SendNotificationToUserAsync(string userId, string message);
        Task<List<UserProfile>> GetUsersByDepartmentAsync(string department);
        Task<Dictionary<UserRole, int>> GetUserCountByRoleAsync();
        Task<Dictionary<UserStatus, int>> GetUserCountByStatusAsync();
        
        // Additional methods for enhanced functionality
        Task<UserSearchResult> GetUsersAsync(UserSearchCriteria criteria);
        Task<UserStatistics> GetUserStatisticsAsync();
        Task<List<string>> GetDepartmentsAsync();
        Task<List<string>> GetRolesAsync();
        Task<List<string>> GetJobTitlesAsync();
        Task<List<string>> GetOfficeLocationsAsync();
        Task<bool> AssignRoleAsync(string userId, string role);
        Task<bool> RemoveRoleAsync(string userId, string role);
        Task<List<UserProfile>> GetUsersByJobTitleAsync(string jobTitle);
        Task<List<UserProfile>> GetUsersByOfficeLocationAsync(string officeLocation);
        Task<bool> BulkUpdateUsersAsync(List<UserProfile> users);
        Task<bool> BulkDeleteUsersAsync(List<string> userIds);
        Task<bool> ExportUsersAsync(string filePath, UserSearchCriteria? criteria = null);
        Task<bool> ImportUsersAsync(string filePath);
    }
} 
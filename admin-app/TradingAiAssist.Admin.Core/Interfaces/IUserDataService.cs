using TradingAiAssist.Admin.Core.Models;

namespace TradingAiAssist.Admin.Core.Interfaces
{
    public interface IUserDataService
    {
        // Basic CRUD operations
        Task<List<UserProfile>> GetUsersAsync(string accessToken);
        Task<UserProfile?> GetUserByIdAsync(string userId, string accessToken);
        Task<UserProfile?> GetUserByEmailAsync(string email, string accessToken);
        Task<bool> CreateUserAsync(UserProfile user, string accessToken);
        Task<bool> UpdateUserAsync(UserProfile user, string accessToken);
        Task<bool> DeleteUserAsync(string userId, string accessToken);
        
        // Search and filtering
        Task<List<UserProfile>> SearchUsersAsync(string searchTerm, string accessToken);
        Task<UserSearchResult> GetUsersAsync(UserSearchCriteria criteria, string accessToken);
        
        // User management
        Task<bool> SuspendUserAsync(string userId, string accessToken);
        Task<bool> ActivateUserAsync(string userId, string accessToken);
        Task<bool> ChangeUserRoleAsync(string userId, UserRole newRole, string accessToken);
        Task<bool> ResetUserPasswordAsync(string userId, string accessToken);
        
        // Role management
        Task<bool> AssignRoleAsync(string userId, string role, string accessToken);
        Task<bool> RemoveRoleAsync(string userId, string role, string accessToken);
        
        // Filtering by criteria
        Task<List<UserProfile>> GetUsersByRoleAsync(UserRole role, string accessToken);
        Task<List<UserProfile>> GetUsersByStatusAsync(UserStatus status, string accessToken);
        Task<List<UserProfile>> GetUsersByDepartmentAsync(string department, string accessToken);
        Task<List<UserProfile>> GetUsersByJobTitleAsync(string jobTitle, string accessToken);
        Task<List<UserProfile>> GetUsersByOfficeLocationAsync(string officeLocation, string accessToken);
        
        // Statistics and counts
        Task<int> GetTotalUserCountAsync(string accessToken);
        Task<int> GetActiveUserCountAsync(string accessToken);
        Task<UserStatistics> GetUserStatisticsAsync(string accessToken);
        Task<Dictionary<UserRole, int>> GetUserCountByRoleAsync(string accessToken);
        Task<Dictionary<UserStatus, int>> GetUserCountByStatusAsync(string accessToken);
        
        // Metadata
        Task<List<string>> GetDepartmentsAsync(string accessToken);
        Task<List<string>> GetRolesAsync(string accessToken);
        Task<List<string>> GetJobTitlesAsync(string accessToken);
        Task<List<string>> GetOfficeLocationsAsync(string accessToken);
        
        // Bulk operations
        Task<bool> BulkUpdateUsersAsync(List<UserProfile> users, string accessToken);
        Task<bool> BulkDeleteUsersAsync(List<string> userIds, string accessToken);
        
        // Import/Export
        Task<bool> ExportUsersAsync(string filePath, UserSearchCriteria? criteria, string accessToken);
        Task<bool> ImportUsersAsync(string filePath, string accessToken);
        
        // Notifications
        Task<bool> SendNotificationToUserAsync(string userId, string message, string accessToken);
    }
} 
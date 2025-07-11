using TradingAiAssist.Admin.Core.Models;

namespace TradingAiAssist.Admin.Core.Interfaces
{
    public interface IUserDataService
    {
        Task<List<UserProfile>> GetUsersAsync(string accessToken);
        Task<UserProfile?> GetUserByIdAsync(string userId, string accessToken);
        Task<bool> UpdateUserAsync(UserProfile user, string accessToken);
        Task<bool> DeleteUserAsync(string userId, string accessToken);
        Task<List<UserProfile>> SearchUsersAsync(string searchTerm, string accessToken);
        Task<bool> AssignRoleAsync(string userId, string role, string accessToken);
        Task<bool> RemoveRoleAsync(string userId, string role, string accessToken);
    }
} 
using TradingAiAssist.Admin.Core.Models;

namespace TradingAiAssist.Admin.Core.Interfaces
{
    public interface IAuthenticationService
    {
        Task<bool> IsAuthenticatedAsync();
        Task<UserProfile?> GetCurrentUserAsync();
        Task<bool> LoginAsync(string username, string password);
        Task<bool> LoginWithAzureAdAsync();
        Task LogoutAsync();
        Task<bool> HasPermissionAsync(string permission);
        Task<bool> HasRoleAsync(UserRole role);
        Task<List<string>> GetUserPermissionsAsync();
        Task<UserRole> GetUserRoleAsync();
        Task<bool> ValidateTokenAsync(string token);
        Task<string?> GetAccessTokenAsync();
        Task RefreshTokenAsync();
    }
} 
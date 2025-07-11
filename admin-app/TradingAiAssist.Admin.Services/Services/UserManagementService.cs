using Microsoft.Extensions.Logging;
using TradingAiAssist.Admin.Core.Interfaces;
using TradingAiAssist.Admin.Core.Models;

namespace TradingAiAssist.Admin.Services.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly ILogger<UserManagementService> _logger;
        private readonly IAuthenticationService _authService;
        private readonly IUserDataService _userDataService;

        public UserManagementService(
            ILogger<UserManagementService> logger,
            IAuthenticationService authService,
            IUserDataService userDataService)
        {
            _logger = logger;
            _authService = authService;
            _userDataService = userDataService;
        }

        public async Task<List<UserProfile>> GetAllUsersAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving all users");
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return new List<UserProfile>();
                }

                var users = await _userDataService.GetUsersAsync(authResult.AccessToken!);
                _logger.LogInformation("Retrieved {Count} users", users.Count);
                
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve users");
                return new List<UserProfile>();
            }
        }

        public async Task<UserProfile?> GetUserByIdAsync(string userId)
        {
            try
            {
                _logger.LogInformation("Retrieving user with ID: {UserId}", userId);
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return null;
                }

                var user = await _userDataService.GetUserByIdAsync(userId, authResult.AccessToken!);
                if (user != null)
                {
                    _logger.LogInformation("Retrieved user: {UserName}", user.DisplayName);
                }
                else
                {
                    _logger.LogWarning("User not found with ID: {UserId}", userId);
                }
                
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve user with ID: {UserId}", userId);
                return null;
            }
        }

        public async Task<UserProfile?> GetCurrentUserAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving current user profile");
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return null;
                }

                var userProfile = await _authService.GetUserProfileAsync(authResult.AccessToken!);
                if (userProfile != null)
                {
                    _logger.LogInformation("Retrieved current user: {UserName}", userProfile.DisplayName);
                }
                
                return userProfile;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve current user profile");
                return null;
            }
        }

        public async Task<bool> UpdateUserAsync(UserProfile user)
        {
            try
            {
                _logger.LogInformation("Updating user: {UserId}", user.Id);
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return false;
                }

                var success = await _userDataService.UpdateUserAsync(user, authResult.AccessToken!);
                if (success)
                {
                    _logger.LogInformation("Successfully updated user: {UserName}", user.DisplayName);
                }
                else
                {
                    _logger.LogWarning("Failed to update user: {UserId}", user.Id);
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update user: {UserId}", user.Id);
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            try
            {
                _logger.LogInformation("Deleting user: {UserId}", userId);
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return false;
                }

                var success = await _userDataService.DeleteUserAsync(userId, authResult.AccessToken!);
                if (success)
                {
                    _logger.LogInformation("Successfully deleted user: {UserId}", userId);
                }
                else
                {
                    _logger.LogWarning("Failed to delete user: {UserId}", userId);
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete user: {UserId}", userId);
                return false;
            }
        }

        public async Task<List<UserProfile>> SearchUsersAsync(string searchTerm)
        {
            try
            {
                _logger.LogInformation("Searching users with term: {SearchTerm}", searchTerm);
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return new List<UserProfile>();
                }

                var users = await _userDataService.SearchUsersAsync(searchTerm, authResult.AccessToken!);
                _logger.LogInformation("Found {Count} users matching search term", users.Count);
                
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to search users with term: {SearchTerm}", searchTerm);
                return new List<UserProfile>();
            }
        }

        public async Task<bool> AssignRoleAsync(string userId, string role)
        {
            try
            {
                _logger.LogInformation("Assigning role {Role} to user: {UserId}", role, userId);
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return false;
                }

                var success = await _userDataService.AssignRoleAsync(userId, role, authResult.AccessToken!);
                if (success)
                {
                    _logger.LogInformation("Successfully assigned role {Role} to user: {UserId}", role, userId);
                }
                else
                {
                    _logger.LogWarning("Failed to assign role {Role} to user: {UserId}", role, userId);
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to assign role {Role} to user: {UserId}", role, userId);
                return false;
            }
        }

        public async Task<bool> RemoveRoleAsync(string userId, string role)
        {
            try
            {
                _logger.LogInformation("Removing role {Role} from user: {UserId}", role, userId);
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return false;
                }

                var success = await _userDataService.RemoveRoleAsync(userId, role, authResult.AccessToken!);
                if (success)
                {
                    _logger.LogInformation("Successfully removed role {Role} from user: {UserId}", role, userId);
                }
                else
                {
                    _logger.LogWarning("Failed to remove role {Role} from user: {UserId}", role, userId);
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove role {Role} from user: {UserId}", role, userId);
                return false;
            }
        }
    }
} 
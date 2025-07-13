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

        public async Task<UserSearchResult> GetUsersAsync(UserSearchCriteria criteria)
        {
            try
            {
                _logger.LogInformation("Searching users with criteria");
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return new UserSearchResult();
                }

                var result = await _userDataService.GetUsersAsync(criteria, authResult.AccessToken!);
                _logger.LogInformation("Found {Count} users matching criteria", result.TotalCount);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to search users with criteria");
                return new UserSearchResult();
            }
        }

        public async Task<UserStatistics> GetUserStatisticsAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving user statistics");
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return new UserStatistics();
                }

                var stats = await _userDataService.GetUserStatisticsAsync(authResult.AccessToken!);
                _logger.LogInformation("Retrieved user statistics with {TotalUsers} total users", stats.TotalUsers);
                
                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve user statistics");
                return new UserStatistics();
            }
        }

        public async Task<List<string>> GetDepartmentsAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving departments");
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return new List<string>();
                }

                var departments = await _userDataService.GetDepartmentsAsync(authResult.AccessToken!);
                _logger.LogInformation("Retrieved {Count} departments", departments.Count);
                
                return departments;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve departments");
                return new List<string>();
            }
        }

        public async Task<List<string>> GetRolesAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving roles");
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return new List<string>();
                }

                var roles = await _userDataService.GetRolesAsync(authResult.AccessToken!);
                _logger.LogInformation("Retrieved {Count} roles", roles.Count);
                
                return roles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve roles");
                return new List<string>();
            }
        }

        public async Task<List<string>> GetJobTitlesAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving job titles");
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return new List<string>();
                }

                var jobTitles = await _userDataService.GetJobTitlesAsync(authResult.AccessToken!);
                _logger.LogInformation("Retrieved {Count} job titles", jobTitles.Count);
                
                return jobTitles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve job titles");
                return new List<string>();
            }
        }

        public async Task<List<string>> GetOfficeLocationsAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving office locations");
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return new List<string>();
                }

                var locations = await _userDataService.GetOfficeLocationsAsync(authResult.AccessToken!);
                _logger.LogInformation("Retrieved {Count} office locations", locations.Count);
                
                return locations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve office locations");
                return new List<string>();
            }
        }

        public async Task<List<UserProfile>> GetUsersByJobTitleAsync(string jobTitle)
        {
            try
            {
                _logger.LogInformation("Retrieving users by job title: {JobTitle}", jobTitle);
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return new List<UserProfile>();
                }

                var users = await _userDataService.GetUsersByJobTitleAsync(jobTitle, authResult.AccessToken!);
                _logger.LogInformation("Retrieved {Count} users with job title {JobTitle}", users.Count, jobTitle);
                
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve users by job title: {JobTitle}", jobTitle);
                return new List<UserProfile>();
            }
        }

        public async Task<List<UserProfile>> GetUsersByOfficeLocationAsync(string officeLocation)
        {
            try
            {
                _logger.LogInformation("Retrieving users by office location: {OfficeLocation}", officeLocation);
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return new List<UserProfile>();
                }

                var users = await _userDataService.GetUsersByOfficeLocationAsync(officeLocation, authResult.AccessToken!);
                _logger.LogInformation("Retrieved {Count} users in office location {OfficeLocation}", users.Count, officeLocation);
                
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve users by office location: {OfficeLocation}", officeLocation);
                return new List<UserProfile>();
            }
        }

        public async Task<bool> BulkUpdateUsersAsync(List<UserProfile> users)
        {
            try
            {
                _logger.LogInformation("Bulk updating {Count} users", users.Count);
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return false;
                }

                var success = await _userDataService.BulkUpdateUsersAsync(users, authResult.AccessToken!);
                if (success)
                {
                    _logger.LogInformation("Successfully bulk updated {Count} users", users.Count);
                }
                else
                {
                    _logger.LogWarning("Failed to bulk update {Count} users", users.Count);
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to bulk update {Count} users", users.Count);
                return false;
            }
        }

        public async Task<bool> BulkDeleteUsersAsync(List<string> userIds)
        {
            try
            {
                _logger.LogInformation("Bulk deleting {Count} users", userIds.Count);
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return false;
                }

                var success = await _userDataService.BulkDeleteUsersAsync(userIds, authResult.AccessToken!);
                if (success)
                {
                    _logger.LogInformation("Successfully bulk deleted {Count} users", userIds.Count);
                }
                else
                {
                    _logger.LogWarning("Failed to bulk delete {Count} users", userIds.Count);
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to bulk delete {Count} users", userIds.Count);
                return false;
            }
        }

        public async Task<bool> ExportUsersAsync(string filePath, UserSearchCriteria? criteria = null)
        {
            try
            {
                _logger.LogInformation("Exporting users to {FilePath}", filePath);
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return false;
                }

                var success = await _userDataService.ExportUsersAsync(filePath, criteria, authResult.AccessToken!);
                if (success)
                {
                    _logger.LogInformation("Successfully exported users to {FilePath}", filePath);
                }
                else
                {
                    _logger.LogWarning("Failed to export users to {FilePath}", filePath);
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export users to {FilePath}", filePath);
                return false;
            }
        }

        public async Task<bool> ImportUsersAsync(string filePath)
        {
            try
            {
                _logger.LogInformation("Importing users from {FilePath}", filePath);
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return false;
                }

                var success = await _userDataService.ImportUsersAsync(filePath, authResult.AccessToken!);
                if (success)
                {
                    _logger.LogInformation("Successfully imported users from {FilePath}", filePath);
                }
                else
                {
                    _logger.LogWarning("Failed to import users from {FilePath}", filePath);
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import users from {FilePath}", filePath);
                return false;
            }
        }

        // Additional methods that need to be implemented
        public async Task<UserProfile?> GetUserByEmailAsync(string email)
        {
            try
            {
                _logger.LogInformation("Retrieving user by email: {Email}", email);
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return null;
                }

                var user = await _userDataService.GetUserByEmailAsync(email, authResult.AccessToken!);
                if (user != null)
                {
                    _logger.LogInformation("Retrieved user by email: {Email}", email);
                }
                else
                {
                    _logger.LogWarning("User not found with email: {Email}", email);
                }
                
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve user by email: {Email}", email);
                return null;
            }
        }

        public async Task<bool> CreateUserAsync(UserProfile user)
        {
            try
            {
                _logger.LogInformation("Creating user: {Email}", user.Email);
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return false;
                }

                var success = await _userDataService.CreateUserAsync(user, authResult.AccessToken!);
                if (success)
                {
                    _logger.LogInformation("Successfully created user: {Email}", user.Email);
                }
                else
                {
                    _logger.LogWarning("Failed to create user: {Email}", user.Email);
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create user: {Email}", user.Email);
                return false;
            }
        }

        public async Task<bool> SuspendUserAsync(string userId)
        {
            try
            {
                _logger.LogInformation("Suspending user: {UserId}", userId);
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return false;
                }

                var success = await _userDataService.SuspendUserAsync(userId, authResult.AccessToken!);
                if (success)
                {
                    _logger.LogInformation("Successfully suspended user: {UserId}", userId);
                }
                else
                {
                    _logger.LogWarning("Failed to suspend user: {UserId}", userId);
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to suspend user: {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> ActivateUserAsync(string userId)
        {
            try
            {
                _logger.LogInformation("Activating user: {UserId}", userId);
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return false;
                }

                var success = await _userDataService.ActivateUserAsync(userId, authResult.AccessToken!);
                if (success)
                {
                    _logger.LogInformation("Successfully activated user: {UserId}", userId);
                }
                else
                {
                    _logger.LogWarning("Failed to activate user: {UserId}", userId);
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to activate user: {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> ChangeUserRoleAsync(string userId, UserRole newRole)
        {
            try
            {
                _logger.LogInformation("Changing role for user: {UserId} to {NewRole}", userId, newRole);
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return false;
                }

                var success = await _userDataService.ChangeUserRoleAsync(userId, newRole, authResult.AccessToken!);
                if (success)
                {
                    _logger.LogInformation("Successfully changed role for user: {UserId} to {NewRole}", userId, newRole);
                }
                else
                {
                    _logger.LogWarning("Failed to change role for user: {UserId} to {NewRole}", userId, newRole);
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to change role for user: {UserId} to {NewRole}", userId, newRole);
                return false;
            }
        }

        public async Task<bool> ResetUserPasswordAsync(string userId)
        {
            try
            {
                _logger.LogInformation("Resetting password for user: {UserId}", userId);
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return false;
                }

                var success = await _userDataService.ResetUserPasswordAsync(userId, authResult.AccessToken!);
                if (success)
                {
                    _logger.LogInformation("Successfully reset password for user: {UserId}", userId);
                }
                else
                {
                    _logger.LogWarning("Failed to reset password for user: {UserId}", userId);
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reset password for user: {UserId}", userId);
                return false;
            }
        }

        public async Task<List<UserProfile>> GetUsersByRoleAsync(UserRole role)
        {
            try
            {
                _logger.LogInformation("Retrieving users by role: {Role}", role);
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return new List<UserProfile>();
                }

                var users = await _userDataService.GetUsersByRoleAsync(role, authResult.AccessToken!);
                _logger.LogInformation("Retrieved {Count} users with role {Role}", users.Count, role);
                
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve users by role: {Role}", role);
                return new List<UserProfile>();
            }
        }

        public async Task<List<UserProfile>> GetUsersByStatusAsync(UserStatus status)
        {
            try
            {
                _logger.LogInformation("Retrieving users by status: {Status}", status);
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return new List<UserProfile>();
                }

                var users = await _userDataService.GetUsersByStatusAsync(status, authResult.AccessToken!);
                _logger.LogInformation("Retrieved {Count} users with status {Status}", users.Count, status);
                
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve users by status: {Status}", status);
                return new List<UserProfile>();
            }
        }

        public async Task<int> GetTotalUserCountAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving total user count");
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return 0;
                }

                var count = await _userDataService.GetTotalUserCountAsync(authResult.AccessToken!);
                _logger.LogInformation("Total user count: {Count}", count);
                
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve total user count");
                return 0;
            }
        }

        public async Task<int> GetActiveUserCountAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving active user count");
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return 0;
                }

                var count = await _userDataService.GetActiveUserCountAsync(authResult.AccessToken!);
                _logger.LogInformation("Active user count: {Count}", count);
                
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve active user count");
                return 0;
            }
        }

        public async Task<bool> SendNotificationToUserAsync(string userId, string message)
        {
            try
            {
                _logger.LogInformation("Sending notification to user: {UserId}", userId);
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return false;
                }

                var success = await _userDataService.SendNotificationToUserAsync(userId, message, authResult.AccessToken!);
                if (success)
                {
                    _logger.LogInformation("Successfully sent notification to user: {UserId}", userId);
                }
                else
                {
                    _logger.LogWarning("Failed to send notification to user: {UserId}", userId);
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification to user: {UserId}", userId);
                return false;
            }
        }

        public async Task<List<UserProfile>> GetUsersByDepartmentAsync(string department)
        {
            try
            {
                _logger.LogInformation("Retrieving users by department: {Department}", department);
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return new List<UserProfile>();
                }

                var users = await _userDataService.GetUsersByDepartmentAsync(department, authResult.AccessToken!);
                _logger.LogInformation("Retrieved {Count} users in department {Department}", users.Count, department);
                
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve users by department: {Department}", department);
                return new List<UserProfile>();
            }
        }

        public async Task<Dictionary<UserRole, int>> GetUserCountByRoleAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving user count by role");
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return new Dictionary<UserRole, int>();
                }

                var counts = await _userDataService.GetUserCountByRoleAsync(authResult.AccessToken!);
                _logger.LogInformation("Retrieved user count by role");
                
                return counts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve user count by role");
                return new Dictionary<UserRole, int>();
            }
        }

        public async Task<Dictionary<UserStatus, int>> GetUserCountByStatusAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving user count by status");
                
                var authResult = await _authService.AuthenticateAsync();
                if (!authResult.IsSuccess)
                {
                    _logger.LogError("Authentication failed: {Error}", authResult.ErrorMessage);
                    return new Dictionary<UserStatus, int>();
                }

                var counts = await _userDataService.GetUserCountByStatusAsync(authResult.AccessToken!);
                _logger.LogInformation("Retrieved user count by status");
                
                return counts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve user count by status");
                return new Dictionary<UserStatus, int>();
            }
        }
    }
} 
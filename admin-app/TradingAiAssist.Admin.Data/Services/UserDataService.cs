using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using TradingAiAssist.Admin.Core.Interfaces;
using TradingAiAssist.Admin.Core.Models;

namespace TradingAiAssist.Admin.Data.Services
{
    public class UserDataService : IUserDataService
    {
        private readonly ILogger<UserDataService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ApiOptions _apiOptions;

        public UserDataService(
            ILogger<UserDataService> logger,
            IHttpClientFactory httpClientFactory,
            IOptions<ApiOptions> apiOptions)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _apiOptions = apiOptions.Value;
        }

        private HttpClient CreateClient()
        {
            return _httpClientFactory.CreateClient("TradingAiAssistApi");
        }

        public async Task<List<UserProfile>> GetUsersAsync(string accessToken)
        {
            try
            {
                _logger.LogInformation("Retrieving users from API");
                
                using var client = CreateClient();
                var request = new HttpRequestMessage(HttpMethod.Get, "/api/users");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var users = JsonSerializer.Deserialize<List<UserProfile>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Retrieved {Count} users from API", users?.Count ?? 0);
                return users ?? new List<UserProfile>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve users from API");
                return new List<UserProfile>();
            }
        }

        public async Task<UserProfile?> GetUserByIdAsync(string userId, string accessToken)
        {
            try
            {
                _logger.LogInformation("Retrieving user by ID: {UserId}", userId);
                
                using var client = CreateClient();
                var request = new HttpRequestMessage(HttpMethod.Get, $"/api/users/{userId}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await client.SendAsync(request);
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("User not found: {UserId}", userId);
                    return null;
                }
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var user = JsonSerializer.Deserialize<UserProfile>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Retrieved user: {UserName}", user?.DisplayName);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve user by ID: {UserId}", userId);
                return null;
            }
        }

        public async Task<bool> UpdateUserAsync(UserProfile user, string accessToken)
        {
            try
            {
                _logger.LogInformation("Updating user: {UserId}", user.Id);
                
                using var client = CreateClient();
                var json = JsonSerializer.Serialize(user);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var request = new HttpRequestMessage(HttpMethod.Put, $"/api/users/{user.Id}")
                {
                    Content = content
                };
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Successfully updated user: {UserName}", user.DisplayName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update user: {UserId}", user.Id);
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(string userId, string accessToken)
        {
            try
            {
                _logger.LogInformation("Deleting user: {UserId}", userId);
                
                using var client = CreateClient();
                var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/users/{userId}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Successfully deleted user: {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete user: {UserId}", userId);
                return false;
            }
        }

        public async Task<List<UserProfile>> SearchUsersAsync(string searchTerm, string accessToken)
        {
            try
            {
                _logger.LogInformation("Searching users with term: {SearchTerm}", searchTerm);
                
                using var client = CreateClient();
                var encodedSearchTerm = Uri.EscapeDataString(searchTerm);
                var request = new HttpRequestMessage(HttpMethod.Get, $"/api/users/search?q={encodedSearchTerm}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var users = JsonSerializer.Deserialize<List<UserProfile>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Found {Count} users matching search term", users?.Count ?? 0);
                return users ?? new List<UserProfile>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to search users with term: {SearchTerm}", searchTerm);
                return new List<UserProfile>();
            }
        }

        public async Task<bool> AssignRoleAsync(string userId, string role, string accessToken)
        {
            try
            {
                _logger.LogInformation("Assigning role {Role} to user: {UserId}", role, userId);
                
                using var client = CreateClient();
                var requestData = new { Role = role };
                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var request = new HttpRequestMessage(HttpMethod.Post, $"/api/users/{userId}/roles")
                {
                    Content = content
                };
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Successfully assigned role {Role} to user: {UserId}", role, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to assign role {Role} to user: {UserId}", role, userId);
                return false;
            }
        }

        public async Task<bool> RemoveRoleAsync(string userId, string role, string accessToken)
        {
            try
            {
                _logger.LogInformation("Removing role {Role} from user: {UserId}", role, userId);
                
                using var client = CreateClient();
                var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/users/{userId}/roles/{role}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Successfully removed role {Role} from user: {UserId}", role, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove role {Role} from user: {UserId}", role, userId);
                return false;
            }
        }

        public async Task<UserProfile?> GetUserByEmailAsync(string email, string accessToken)
        {
            try
            {
                _logger.LogInformation("Retrieving user by email: {Email}", email);
                
                var encodedEmail = Uri.EscapeDataString(email);
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiOptions.BaseUrl}/api/users/email/{encodedEmail}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("User not found with email: {Email}", email);
                    return null;
                }
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var user = JsonSerializer.Deserialize<UserProfile>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Retrieved user by email: {Email}", email);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve user by email: {Email}", email);
                return null;
            }
        }

        public async Task<bool> CreateUserAsync(UserProfile user, string accessToken)
        {
            try
            {
                _logger.LogInformation("Creating user: {Email}", user.Email);
                
                var json = JsonSerializer.Serialize(user);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiOptions.BaseUrl}/api/users")
                {
                    Content = content
                };
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Successfully created user: {Email}", user.Email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create user: {Email}", user.Email);
                return false;
            }
        }

        public async Task<UserSearchResult> GetUsersAsync(UserSearchCriteria criteria, string accessToken)
        {
            try
            {
                _logger.LogInformation("Searching users with criteria");
                
                var json = JsonSerializer.Serialize(criteria);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiOptions.BaseUrl}/api/users/search")
                {
                    Content = content
                };
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<UserSearchResult>(responseJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Found {Count} users matching criteria", result?.TotalCount ?? 0);
                return result ?? new UserSearchResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to search users with criteria");
                return new UserSearchResult();
            }
        }

        public async Task<bool> SuspendUserAsync(string userId, string accessToken)
        {
            try
            {
                _logger.LogInformation("Suspending user: {UserId}", userId);
                
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiOptions.BaseUrl}/api/users/{userId}/suspend");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Successfully suspended user: {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to suspend user: {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> ActivateUserAsync(string userId, string accessToken)
        {
            try
            {
                _logger.LogInformation("Activating user: {UserId}", userId);
                
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiOptions.BaseUrl}/api/users/{userId}/activate");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Successfully activated user: {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to activate user: {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> ChangeUserRoleAsync(string userId, UserRole newRole, string accessToken)
        {
            try
            {
                _logger.LogInformation("Changing role for user: {UserId} to {NewRole}", userId, newRole);
                
                var requestData = new { Role = newRole.ToString() };
                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var request = new HttpRequestMessage(HttpMethod.Put, $"{_apiOptions.BaseUrl}/api/users/{userId}/role")
                {
                    Content = content
                };
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Successfully changed role for user: {UserId} to {NewRole}", userId, newRole);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to change role for user: {UserId} to {NewRole}", userId, newRole);
                return false;
            }
        }

        public async Task<bool> ResetUserPasswordAsync(string userId, string accessToken)
        {
            try
            {
                _logger.LogInformation("Resetting password for user: {UserId}", userId);
                
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiOptions.BaseUrl}/api/users/{userId}/reset-password");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Successfully reset password for user: {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reset password for user: {UserId}", userId);
                return false;
            }
        }

        public async Task<List<UserProfile>> GetUsersByRoleAsync(UserRole role, string accessToken)
        {
            try
            {
                _logger.LogInformation("Retrieving users by role: {Role}", role);
                
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiOptions.BaseUrl}/api/users/role/{role}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var users = JsonSerializer.Deserialize<List<UserProfile>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Retrieved {Count} users with role {Role}", users?.Count ?? 0, role);
                return users ?? new List<UserProfile>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve users by role: {Role}", role);
                return new List<UserProfile>();
            }
        }

        public async Task<List<UserProfile>> GetUsersByStatusAsync(UserStatus status, string accessToken)
        {
            try
            {
                _logger.LogInformation("Retrieving users by status: {Status}", status);
                
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiOptions.BaseUrl}/api/users/status/{status}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var users = JsonSerializer.Deserialize<List<UserProfile>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Retrieved {Count} users with status {Status}", users?.Count ?? 0, status);
                return users ?? new List<UserProfile>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve users by status: {Status}", status);
                return new List<UserProfile>();
            }
        }

        public async Task<List<UserProfile>> GetUsersByDepartmentAsync(string department, string accessToken)
        {
            try
            {
                _logger.LogInformation("Retrieving users by department: {Department}", department);
                
                var encodedDepartment = Uri.EscapeDataString(department);
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiOptions.BaseUrl}/api/users/department/{encodedDepartment}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var users = JsonSerializer.Deserialize<List<UserProfile>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Retrieved {Count} users in department {Department}", users?.Count ?? 0, department);
                return users ?? new List<UserProfile>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve users by department: {Department}", department);
                return new List<UserProfile>();
            }
        }

        public async Task<List<UserProfile>> GetUsersByJobTitleAsync(string jobTitle, string accessToken)
        {
            try
            {
                _logger.LogInformation("Retrieving users by job title: {JobTitle}", jobTitle);
                
                var encodedJobTitle = Uri.EscapeDataString(jobTitle);
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiOptions.BaseUrl}/api/users/jobtitle/{encodedJobTitle}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var users = JsonSerializer.Deserialize<List<UserProfile>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Retrieved {Count} users with job title {JobTitle}", users?.Count ?? 0, jobTitle);
                return users ?? new List<UserProfile>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve users by job title: {JobTitle}", jobTitle);
                return new List<UserProfile>();
            }
        }

        public async Task<List<UserProfile>> GetUsersByOfficeLocationAsync(string officeLocation, string accessToken)
        {
            try
            {
                _logger.LogInformation("Retrieving users by office location: {OfficeLocation}", officeLocation);
                
                var encodedLocation = Uri.EscapeDataString(officeLocation);
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiOptions.BaseUrl}/api/users/location/{encodedLocation}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var users = JsonSerializer.Deserialize<List<UserProfile>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Retrieved {Count} users in office location {OfficeLocation}", users?.Count ?? 0, officeLocation);
                return users ?? new List<UserProfile>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve users by office location: {OfficeLocation}", officeLocation);
                return new List<UserProfile>();
            }
        }

        public async Task<int> GetTotalUserCountAsync(string accessToken)
        {
            try
            {
                _logger.LogInformation("Retrieving total user count");
                
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiOptions.BaseUrl}/api/users/count/total");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<CountResult>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Total user count: {Count}", result?.Count ?? 0);
                return result?.Count ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve total user count");
                return 0;
            }
        }

        public async Task<int> GetActiveUserCountAsync(string accessToken)
        {
            try
            {
                _logger.LogInformation("Retrieving active user count");
                
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiOptions.BaseUrl}/api/users/count/active");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<CountResult>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Active user count: {Count}", result?.Count ?? 0);
                return result?.Count ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve active user count");
                return 0;
            }
        }

        public async Task<UserStatistics> GetUserStatisticsAsync(string accessToken)
        {
            try
            {
                _logger.LogInformation("Retrieving user statistics");
                
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiOptions.BaseUrl}/api/users/statistics");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var stats = JsonSerializer.Deserialize<UserStatistics>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Retrieved user statistics with {TotalUsers} total users", stats?.TotalUsers ?? 0);
                return stats ?? new UserStatistics();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve user statistics");
                return new UserStatistics();
            }
        }

        public async Task<Dictionary<UserRole, int>> GetUserCountByRoleAsync(string accessToken)
        {
            try
            {
                _logger.LogInformation("Retrieving user count by role");
                
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiOptions.BaseUrl}/api/users/count/roles");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var counts = JsonSerializer.Deserialize<Dictionary<UserRole, int>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Retrieved user count by role");
                return counts ?? new Dictionary<UserRole, int>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve user count by role");
                return new Dictionary<UserRole, int>();
            }
        }

        public async Task<Dictionary<UserStatus, int>> GetUserCountByStatusAsync(string accessToken)
        {
            try
            {
                _logger.LogInformation("Retrieving user count by status");
                
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiOptions.BaseUrl}/api/users/count/status");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var counts = JsonSerializer.Deserialize<Dictionary<UserStatus, int>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Retrieved user count by status");
                return counts ?? new Dictionary<UserStatus, int>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve user count by status");
                return new Dictionary<UserStatus, int>();
            }
        }

        public async Task<List<string>> GetDepartmentsAsync(string accessToken)
        {
            try
            {
                _logger.LogInformation("Retrieving departments");
                
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiOptions.BaseUrl}/api/users/departments");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var departments = JsonSerializer.Deserialize<List<string>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Retrieved {Count} departments", departments?.Count ?? 0);
                return departments ?? new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve departments");
                return new List<string>();
            }
        }

        public async Task<List<string>> GetRolesAsync(string accessToken)
        {
            try
            {
                _logger.LogInformation("Retrieving roles");
                
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiOptions.BaseUrl}/api/users/roles");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var roles = JsonSerializer.Deserialize<List<string>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Retrieved {Count} roles", roles?.Count ?? 0);
                return roles ?? new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve roles");
                return new List<string>();
            }
        }

        public async Task<List<string>> GetJobTitlesAsync(string accessToken)
        {
            try
            {
                _logger.LogInformation("Retrieving job titles");
                
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiOptions.BaseUrl}/api/users/jobtitles");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var jobTitles = JsonSerializer.Deserialize<List<string>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Retrieved {Count} job titles", jobTitles?.Count ?? 0);
                return jobTitles ?? new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve job titles");
                return new List<string>();
            }
        }

        public async Task<List<string>> GetOfficeLocationsAsync(string accessToken)
        {
            try
            {
                _logger.LogInformation("Retrieving office locations");
                
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiOptions.BaseUrl}/api/users/locations");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var locations = JsonSerializer.Deserialize<List<string>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Retrieved {Count} office locations", locations?.Count ?? 0);
                return locations ?? new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve office locations");
                return new List<string>();
            }
        }

        public async Task<bool> BulkUpdateUsersAsync(List<UserProfile> users, string accessToken)
        {
            try
            {
                _logger.LogInformation("Bulk updating {Count} users", users.Count);
                
                var json = JsonSerializer.Serialize(users);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var request = new HttpRequestMessage(HttpMethod.Put, $"{_apiOptions.BaseUrl}/api/users/bulk")
                {
                    Content = content
                };
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Successfully bulk updated {Count} users", users.Count);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to bulk update {Count} users", users.Count);
                return false;
            }
        }

        public async Task<bool> BulkDeleteUsersAsync(List<string> userIds, string accessToken)
        {
            try
            {
                _logger.LogInformation("Bulk deleting {Count} users", userIds.Count);
                
                var json = JsonSerializer.Serialize(userIds);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var request = new HttpRequestMessage(HttpMethod.Delete, $"{_apiOptions.BaseUrl}/api/users/bulk")
                {
                    Content = content
                };
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Successfully bulk deleted {Count} users", userIds.Count);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to bulk delete {Count} users", userIds.Count);
                return false;
            }
        }

        public async Task<bool> ExportUsersAsync(string filePath, UserSearchCriteria? criteria, string accessToken)
        {
            try
            {
                _logger.LogInformation("Exporting users to {FilePath}", filePath);
                
                var requestData = criteria ?? new UserSearchCriteria();
                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiOptions.BaseUrl}/api/users/export")
                {
                    Content = content
                };
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var fileBytes = await response.Content.ReadAsByteArrayAsync();
                await File.WriteAllBytesAsync(filePath, fileBytes);

                _logger.LogInformation("Successfully exported users to {FilePath}", filePath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export users to {FilePath}", filePath);
                return false;
            }
        }

        public async Task<bool> ImportUsersAsync(string filePath, string accessToken)
        {
            try
            {
                _logger.LogInformation("Importing users from {FilePath}", filePath);
                
                var fileBytes = await File.ReadAllBytesAsync(filePath);
                var content = new ByteArrayContent(fileBytes);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiOptions.BaseUrl}/api/users/import")
                {
                    Content = content
                };
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Successfully imported users from {FilePath}", filePath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import users from {FilePath}", filePath);
                return false;
            }
        }

        public async Task<bool> SendNotificationToUserAsync(string userId, string message, string accessToken)
        {
            try
            {
                _logger.LogInformation("Sending notification to user: {UserId}", userId);
                
                var requestData = new { Message = message };
                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiOptions.BaseUrl}/api/users/{userId}/notify")
                {
                    Content = content
                };
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Successfully sent notification to user: {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification to user: {UserId}", userId);
                return false;
            }
        }
    }

    public class ApiOptions
    {
        public string BaseUrl { get; set; } = "https://api.tradingaiassist.com";
        public int TimeoutSeconds { get; set; } = 30;
        public int MaxRetries { get; set; } = 3;
    }

    public class CountResult
    {
        public int Count { get; set; }
    }
} 
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
        private readonly HttpClient _httpClient;
        private readonly ApiOptions _apiOptions;

        public UserDataService(
            ILogger<UserDataService> logger,
            HttpClient httpClient,
            IOptions<ApiOptions> apiOptions)
        {
            _logger = logger;
            _httpClient = httpClient;
            _apiOptions = apiOptions.Value;
        }

        public async Task<List<UserProfile>> GetUsersAsync(string accessToken)
        {
            try
            {
                _logger.LogInformation("Retrieving users from API");
                
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiOptions.BaseUrl}/api/users");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
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
                
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiOptions.BaseUrl}/api/users/{userId}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
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
                
                var json = JsonSerializer.Serialize(user);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var request = new HttpRequestMessage(HttpMethod.Put, $"{_apiOptions.BaseUrl}/api/users/{user.Id}")
                {
                    Content = content
                };
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
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
                
                var request = new HttpRequestMessage(HttpMethod.Delete, $"{_apiOptions.BaseUrl}/api/users/{userId}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
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
                
                var encodedSearchTerm = Uri.EscapeDataString(searchTerm);
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiOptions.BaseUrl}/api/users/search?q={encodedSearchTerm}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
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
                
                var requestData = new { Role = role };
                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiOptions.BaseUrl}/api/users/{userId}/roles")
                {
                    Content = content
                };
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
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
                
                var request = new HttpRequestMessage(HttpMethod.Delete, $"{_apiOptions.BaseUrl}/api/users/{userId}/roles/{role}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
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
    }

    public class ApiOptions
    {
        public string BaseUrl { get; set; } = "https://api.tradingaiassist.com";
        public int TimeoutSeconds { get; set; } = 30;
        public int MaxRetries { get; set; } = 3;
    }
} 
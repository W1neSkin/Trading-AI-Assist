using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using System.Text.Json;
using TradingAiAssist.Admin.Core.Interfaces;
using TradingAiAssist.Admin.Core.Models;

namespace TradingAiAssist.Admin.AzureAd.Services
{
    public class AzureAdService : IAuthenticationService
    {
        private readonly ILogger<AzureAdService> _logger;
        private readonly AzureAdOptions _options;
        private readonly IPublicClientApplication _msalClient;
        private readonly IMemoryCache _cache;

        public AzureAdService(
            ILogger<AzureAdService> logger,
            IOptions<AzureAdOptions> options,
            IMemoryCache cache)
        {
            _logger = logger;
            _options = options.Value;
            _cache = cache;

            _msalClient = PublicClientApplicationBuilder
                .Create(_options.ClientId)
                .WithAuthority(_options.Authority)
                .WithRedirectUri(_options.RedirectUri)
                .Build();
        }

        public async Task<AuthenticationResult> AuthenticateAsync()
        {
            try
            {
                _logger.LogInformation("Starting Azure AD authentication");

                var scopes = new[] { "User.Read", "Directory.Read.All" };
                var accounts = await _msalClient.GetAccountsAsync();
                
                AuthenticationResult result;
                
                if (accounts.Any())
                {
                    // Try to acquire token silently first
                    try
                    {
                        result = await _msalClient.AcquireTokenSilent(scopes, accounts.First()).ExecuteAsync();
                        _logger.LogInformation("Token acquired silently for user: {User}", result.Account.Username);
                    }
                    catch (MsalUiRequiredException)
                    {
                        // Silent token acquisition failed, need interactive login
                        result = await _msalClient.AcquireTokenInteractive(scopes).ExecuteAsync();
                        _logger.LogInformation("Interactive authentication completed for user: {User}", result.Account.Username);
                    }
                }
                else
                {
                    // No cached accounts, perform interactive login
                    result = await _msalClient.AcquireTokenInteractive(scopes).ExecuteAsync();
                    _logger.LogInformation("Interactive authentication completed for user: {User}", result.Account.Username);
                }

                // Cache the token
                var cacheKey = $"token_{result.Account.Username}";
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(50)); // Cache for 50 minutes (tokens expire in 60)

                return new AuthenticationResult
                {
                    IsSuccess = true,
                    AccessToken = result.AccessToken,
                    UserName = result.Account.Username,
                    ExpiresOn = result.ExpiresOn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Authentication failed");
                return new AuthenticationResult
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<UserProfile?> GetUserProfileAsync(string accessToken)
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await httpClient.GetAsync("https://graph.microsoft.com/v1.0/me");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var userData = JsonSerializer.Deserialize<JsonElement>(json);

                return new UserProfile
                {
                    Id = userData.GetProperty("id").GetString() ?? "",
                    DisplayName = userData.GetProperty("displayName").GetString() ?? "",
                    Email = userData.GetProperty("userPrincipalName").GetString() ?? "",
                    GivenName = userData.GetProperty("givenName").GetString() ?? "",
                    Surname = userData.GetProperty("surname").GetString() ?? "",
                    JobTitle = userData.TryGetProperty("jobTitle", out var jobTitle) ? jobTitle.GetString() : "",
                    Department = userData.TryGetProperty("department", out var dept) ? dept.GetString() : "",
                    OfficeLocation = userData.TryGetProperty("officeLocation", out var office) ? office.GetString() : "",
                    PhoneNumber = userData.TryGetProperty("businessPhones", out var phones) && phones.GetArrayLength() > 0 
                        ? phones[0].GetString() : ""
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get user profile");
                return null;
            }
        }

        public async Task<List<UserProfile>> GetUsersAsync(string accessToken)
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await httpClient.GetAsync("https://graph.microsoft.com/v1.0/users?$select=id,displayName,userPrincipalName,givenName,surname,jobTitle,department,officeLocation,businessPhones");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<JsonElement>(json);
                var users = new List<UserProfile>();

                foreach (var user in data.GetProperty("value").EnumerateArray())
                {
                    users.Add(new UserProfile
                    {
                        Id = user.GetProperty("id").GetString() ?? "",
                        DisplayName = user.GetProperty("displayName").GetString() ?? "",
                        Email = user.GetProperty("userPrincipalName").GetString() ?? "",
                        GivenName = user.GetProperty("givenName").GetString() ?? "",
                        Surname = user.GetProperty("surname").GetString() ?? "",
                        JobTitle = user.TryGetProperty("jobTitle", out var jobTitle) ? jobTitle.GetString() : "",
                        Department = user.TryGetProperty("department", out var dept) ? dept.GetString() : "",
                        OfficeLocation = user.TryGetProperty("officeLocation", out var office) ? office.GetString() : "",
                        PhoneNumber = user.TryGetProperty("businessPhones", out var phones) && phones.GetArrayLength() > 0 
                            ? phones[0].GetString() : ""
                    });
                }

                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get users");
                return new List<UserProfile>();
            }
        }

        public async Task<bool> ValidateTokenAsync(string accessToken)
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await httpClient.GetAsync("https://graph.microsoft.com/v1.0/me");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                var accounts = await _msalClient.GetAccountsAsync();
                foreach (var account in accounts)
                {
                    await _msalClient.RemoveAsync(account);
                }

                // Clear cache
                _cache.Clear();
                _logger.LogInformation("User logged out successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
            }
        }
    }

    public class AzureAdOptions
    {
        public string ClientId { get; set; } = "";
        public string Authority { get; set; } = "https://login.microsoftonline.com/common";
        public string RedirectUri { get; set; } = "http://localhost";
        public string TenantId { get; set; } = "";
    }
} 
namespace TradingAiAssist.Admin.Core.Models
{
    public class AuthenticationResult
    {
        public bool IsSuccess { get; set; }
        public string? AccessToken { get; set; }
        public string? UserName { get; set; }
        public DateTimeOffset? ExpiresOn { get; set; }
        public string? ErrorMessage { get; set; }
        public UserProfile? UserProfile { get; set; }
    }
} 
using System.ComponentModel.DataAnnotations;

namespace TradingAiAssist.Admin.Core.Models
{
    public class UserProfile
    {
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string DisplayName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        
        public UserRole Role { get; set; }
        
        public UserStatus Status { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? LastLoginAt { get; set; }
        
        public bool IsActive { get; set; }
        
        public string? PhoneNumber { get; set; }
        
        public string? Department { get; set; }
        
        public string? JobTitle { get; set; }
        
        public string? OfficeLocation { get; set; }
        
        public List<string> Permissions { get; set; } = new();
        
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public enum UserRole
    {
        SuperAdmin,
        Admin,
        Support,
        Compliance,
        Financial,
        AIAdmin,
        Viewer
    }

    public enum UserStatus
    {
        Active,
        Inactive,
        Suspended,
        Pending,
        Banned
    }
} 
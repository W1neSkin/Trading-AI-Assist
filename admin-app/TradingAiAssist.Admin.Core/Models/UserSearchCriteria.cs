using System.ComponentModel.DataAnnotations;

namespace TradingAiAssist.Admin.Core.Models
{
    /// <summary>
    /// Criteria for searching and filtering users
    /// </summary>
    public class UserSearchCriteria
    {
        /// <summary>
        /// Search term for name, email, or department
        /// </summary>
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Filter by user role
        /// </summary>
        public UserRole? Role { get; set; }

        /// <summary>
        /// Filter by user status
        /// </summary>
        public UserStatus? Status { get; set; }

        /// <summary>
        /// Filter by department
        /// </summary>
        public string? Department { get; set; }

        /// <summary>
        /// Filter by job title
        /// </summary>
        public string? JobTitle { get; set; }

        /// <summary>
        /// Filter by office location
        /// </summary>
        public string? OfficeLocation { get; set; }

        /// <summary>
        /// Filter by creation date range - start date
        /// </summary>
        public DateTime? CreatedFrom { get; set; }

        /// <summary>
        /// Filter by creation date range - end date
        /// </summary>
        public DateTime? CreatedTo { get; set; }

        /// <summary>
        /// Filter by last login date range - start date
        /// </summary>
        public DateTime? LastLoginFrom { get; set; }

        /// <summary>
        /// Filter by last login date range - end date
        /// </summary>
        public DateTime? LastLoginTo { get; set; }

        /// <summary>
        /// Page number for pagination (1-based)
        /// </summary>
        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Page size for pagination
        /// </summary>
        [Range(1, 100)]
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// Sort field
        /// </summary>
        public string? SortBy { get; set; }

        /// <summary>
        /// Sort direction (asc/desc)
        /// </summary>
        public string? SortDirection { get; set; } = "asc";

        /// <summary>
        /// Whether to include inactive users
        /// </summary>
        public bool IncludeInactive { get; set; } = false;

        /// <summary>
        /// Whether to include suspended users
        /// </summary>
        public bool IncludeSuspended { get; set; } = false;
    }

    /// <summary>
    /// Result of a user search operation
    /// </summary>
    public class UserSearchResult
    {
        /// <summary>
        /// List of users matching the search criteria
        /// </summary>
        public List<UserProfile> Users { get; set; } = new List<UserProfile>();

        /// <summary>
        /// Total number of users matching the criteria
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Current page number
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Page size used
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total number of pages
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Whether there are more pages
        /// </summary>
        public bool HasNextPage { get; set; }

        /// <summary>
        /// Whether there are previous pages
        /// </summary>
        public bool HasPreviousPage { get; set; }
    }
} 
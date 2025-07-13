namespace TradingAiAssist.Admin.Core.Models
{
    /// <summary>
    /// User statistics and metrics
    /// </summary>
    public class UserStatistics
    {
        /// <summary>
        /// Total number of users
        /// </summary>
        public int TotalUsers { get; set; }

        /// <summary>
        /// Number of active users
        /// </summary>
        public int ActiveUsers { get; set; }

        /// <summary>
        /// Number of inactive users
        /// </summary>
        public int InactiveUsers { get; set; }

        /// <summary>
        /// Number of suspended users
        /// </summary>
        public int SuspendedUsers { get; set; }

        /// <summary>
        /// Number of new users in the last 30 days
        /// </summary>
        public int NewUsersLast30Days { get; set; }

        /// <summary>
        /// Number of users who logged in today
        /// </summary>
        public int UsersLoggedInToday { get; set; }

        /// <summary>
        /// Number of users who logged in this week
        /// </summary>
        public int UsersLoggedInThisWeek { get; set; }

        /// <summary>
        /// Number of users who logged in this month
        /// </summary>
        public int UsersLoggedInThisMonth { get; set; }

        /// <summary>
        /// User count by role
        /// </summary>
        public Dictionary<UserRole, int> UsersByRole { get; set; } = new Dictionary<UserRole, int>();

        /// <summary>
        /// User count by status
        /// </summary>
        public Dictionary<UserStatus, int> UsersByStatus { get; set; } = new Dictionary<UserStatus, int>();

        /// <summary>
        /// User count by department
        /// </summary>
        public Dictionary<string, int> UsersByDepartment { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Top departments by user count
        /// </summary>
        public List<DepartmentStats> TopDepartments { get; set; } = new List<DepartmentStats>();

        /// <summary>
        /// Recent user activity
        /// </summary>
        public List<UserActivity> RecentActivity { get; set; } = new List<UserActivity>();

        /// <summary>
        /// User growth over time
        /// </summary>
        public List<UserGrowthData> GrowthData { get; set; } = new List<UserGrowthData>();
    }

    /// <summary>
    /// Department statistics
    /// </summary>
    public class DepartmentStats
    {
        /// <summary>
        /// Department name
        /// </summary>
        public string Department { get; set; } = "";

        /// <summary>
        /// Number of users in the department
        /// </summary>
        public int UserCount { get; set; }

        /// <summary>
        /// Percentage of total users
        /// </summary>
        public double Percentage { get; set; }

        /// <summary>
        /// Average user activity in the department
        /// </summary>
        public double AverageActivity { get; set; }
    }

    /// <summary>
    /// User activity information
    /// </summary>
    public class UserActivity
    {
        /// <summary>
        /// User ID
        /// </summary>
        public string UserId { get; set; } = "";

        /// <summary>
        /// User display name
        /// </summary>
        public string DisplayName { get; set; } = "";

        /// <summary>
        /// Activity type
        /// </summary>
        public string ActivityType { get; set; } = "";

        /// <summary>
        /// Activity timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Activity description
        /// </summary>
        public string Description { get; set; } = "";
    }

    /// <summary>
    /// User growth data over time
    /// </summary>
    public class UserGrowthData
    {
        /// <summary>
        /// Date
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Total users on this date
        /// </summary>
        public int TotalUsers { get; set; }

        /// <summary>
        /// New users on this date
        /// </summary>
        public int NewUsers { get; set; }

        /// <summary>
        /// Active users on this date
        /// </summary>
        public int ActiveUsers { get; set; }
    }
} 
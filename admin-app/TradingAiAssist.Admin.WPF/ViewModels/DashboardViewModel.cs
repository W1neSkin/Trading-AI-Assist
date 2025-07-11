using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TradingAiAssist.Admin.Core.Models;
using TradingAiAssist.Admin.Core.Interfaces;
using TradingAiAssist.Admin.WPF.Services;
using System.Collections.Generic; // Added for EqualityComparer
using System; // Added for DateTime
using System.Threading.Tasks; // Added for Task

namespace TradingAiAssist.Admin.WPF.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly IUserManagementService _userManagementService;
        private readonly IAiAnalyticsService _aiAnalyticsService;
        private readonly ISystemHealthService _systemHealthService;
        private readonly INotificationService _notificationService;
        private readonly INavigationService _navigationService;
        private DateTime _lastUpdated;
        private bool _isLoading;

        public DashboardViewModel(
            IUserManagementService userManagementService,
            IAiAnalyticsService aiAnalyticsService,
            ISystemHealthService systemHealthService,
            INotificationService notificationService,
            INavigationService navigationService)
        {
            _userManagementService = userManagementService;
            _aiAnalyticsService = aiAnalyticsService;
            _systemHealthService = systemHealthService;
            _notificationService = notificationService;
            _navigationService = navigationService;

            KpiCards = new ObservableCollection<KpiCardViewModel>();
            SystemStatus = new ObservableCollection<SystemStatusViewModel>();
            RecentAlerts = new ObservableCollection<AlertViewModel>();

            RefreshCommand = new RelayCommand(async () => await RefreshDashboardAsync());
            ViewAllAlertsCommand = new RelayCommand(() => NavigateToAlerts());
            NavigateToUsersCommand = new RelayCommand(() => NavigateToUsers());
            NavigateToAnalyticsCommand = new RelayCommand(() => NavigateToAnalytics());
            NavigateToHealthCommand = new RelayCommand(() => NavigateToHealth());
            NavigateToSettingsCommand = new RelayCommand(() => NavigateToSettings());

            // Initialize dashboard
            _ = InitializeDashboardAsync();
        }

        public DateTime LastUpdated
        {
            get => _lastUpdated;
            set => SetProperty(ref _lastUpdated, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ObservableCollection<KpiCardViewModel> KpiCards { get; }
        public ObservableCollection<SystemStatusViewModel> SystemStatus { get; }
        public ObservableCollection<AlertViewModel> RecentAlerts { get; }

        public ICommand RefreshCommand { get; }
        public ICommand ViewAllAlertsCommand { get; }
        public ICommand NavigateToUsersCommand { get; }
        public ICommand NavigateToAnalyticsCommand { get; }
        public ICommand NavigateToHealthCommand { get; }
        public ICommand NavigateToSettingsCommand { get; }

        private async Task InitializeDashboardAsync()
        {
            IsLoading = true;
            try
            {
                await LoadKpiCardsAsync();
                await LoadSystemStatusAsync();
                await LoadRecentAlertsAsync();
                LastUpdated = DateTime.Now;
            }
            catch (Exception ex)
            {
                // Handle error - could show a message to user
                System.Diagnostics.Debug.WriteLine($"Error initializing dashboard: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RefreshDashboardAsync()
        {
            await InitializeDashboardAsync();
        }

        private async Task LoadKpiCardsAsync()
        {
            KpiCards.Clear();

            try
            {
                // Load real data from services
                var userStats = await _userManagementService.GetUserStatisticsAsync();
                var aiStats = await _aiAnalyticsService.GetUsageStatisticsAsync();
                var systemStats = await _systemHealthService.GetSystemStatisticsAsync();

                KpiCards.Add(new KpiCardViewModel
                {
                    Title = "Total Users",
                    Value = userStats.TotalUsers.ToString("N0"),
                    Icon = "👥",
                    ChangeText = $"{userStats.GrowthPercentage:F1}%",
                    ChangeValue = "from last month",
                    ChangeColor = userStats.GrowthPercentage >= 0 ? "#4CAF50" : "#F44336"
                });

                KpiCards.Add(new KpiCardViewModel
                {
                    Title = "AI Requests",
                    Value = aiStats.TotalRequests.ToString("N0"),
                    Icon = "🤖",
                    ChangeText = $"{aiStats.RequestGrowthPercentage:F1}%",
                    ChangeValue = "from last week",
                    ChangeColor = aiStats.RequestGrowthPercentage >= 0 ? "#2196F3" : "#F44336"
                });

                KpiCards.Add(new KpiCardViewModel
                {
                    Title = "Total Cost",
                    Value = aiStats.TotalCost.ToString("C"),
                    Icon = "💰",
                    ChangeText = $"{aiStats.CostChangePercentage:F1}%",
                    ChangeValue = "from last month",
                    ChangeColor = aiStats.CostChangePercentage <= 0 ? "#4CAF50" : "#FF9800"
                });

                KpiCards.Add(new KpiCardViewModel
                {
                    Title = "Active Sessions",
                    Value = userStats.ActiveSessions.ToString(),
                    Icon = "🟢",
                    ChangeText = $"{userStats.SessionGrowthPercentage:F1}%",
                    ChangeValue = "from yesterday",
                    ChangeColor = userStats.SessionGrowthPercentage >= 0 ? "#4CAF50" : "#F44336"
                });

                KpiCards.Add(new KpiCardViewModel
                {
                    Title = "System Uptime",
                    Value = $"{systemStats.UptimePercentage:F1}%",
                    Icon = "⚡",
                    ChangeText = $"{systemStats.UptimeChange:F1}%",
                    ChangeValue = "from last week",
                    ChangeColor = systemStats.UptimeChange >= 0 ? "#4CAF50" : "#F44336"
                });

                KpiCards.Add(new KpiCardViewModel
                {
                    Title = "Alerts",
                    Value = systemStats.ActiveAlerts.ToString(),
                    Icon = "⚠️",
                    ChangeText = systemStats.AlertChange.ToString(),
                    ChangeValue = "from yesterday",
                    ChangeColor = systemStats.AlertChange <= 0 ? "#4CAF50" : "#F44336"
                });
            }
            catch (Exception ex)
            {
                // Fallback to mock data if services fail
                LoadMockKpiCards();
                System.Diagnostics.Debug.WriteLine($"Error loading KPI cards: {ex.Message}");
            }
        }

        private void LoadMockKpiCards()
        {
            KpiCards.Add(new KpiCardViewModel
            {
                Title = "Total Users",
                Value = "1,247",
                Icon = "👥",
                ChangeText = "+12%",
                ChangeValue = "from last month",
                ChangeColor = "#4CAF50"
            });

            KpiCards.Add(new KpiCardViewModel
            {
                Title = "AI Requests",
                Value = "45,892",
                Icon = "🤖",
                ChangeText = "+8%",
                ChangeValue = "from last week",
                ChangeColor = "#2196F3"
            });

            KpiCards.Add(new KpiCardViewModel
            {
                Title = "Total Cost",
                Value = "$2,847.50",
                Icon = "💰",
                ChangeText = "-5%",
                ChangeValue = "from last month",
                ChangeColor = "#FF9800"
            });

            KpiCards.Add(new KpiCardViewModel
            {
                Title = "Active Sessions",
                Value = "89",
                Icon = "🟢",
                ChangeText = "+15%",
                ChangeValue = "from yesterday",
                ChangeColor = "#4CAF50"
            });

            KpiCards.Add(new KpiCardViewModel
            {
                Title = "System Uptime",
                Value = "99.8%",
                Icon = "⚡",
                ChangeText = "+0.2%",
                ChangeValue = "from last week",
                ChangeColor = "#4CAF50"
            });

            KpiCards.Add(new KpiCardViewModel
            {
                Title = "Alerts",
                Value = "3",
                Icon = "⚠️",
                ChangeText = "-2",
                ChangeValue = "from yesterday",
                ChangeColor = "#F44336"
            });
        }

        private async Task LoadSystemStatusAsync()
        {
            SystemStatus.Clear();

            try
            {
                var healthStatus = await _systemHealthService.GetSystemHealthAsync();
                
                foreach (var service in healthStatus.Services)
                {
                    SystemStatus.Add(new SystemStatusViewModel
                    {
                        ServiceName = service.Name,
                        Status = service.Status.ToString(),
                        StatusColor = GetStatusColor(service.Status)
                    });
                }
            }
            catch (Exception ex)
            {
                // Fallback to mock data
                LoadMockSystemStatus();
                System.Diagnostics.Debug.WriteLine($"Error loading system status: {ex.Message}");
            }
        }

        private void LoadMockSystemStatus()
        {
            SystemStatus.Add(new SystemStatusViewModel
            {
                ServiceName = "API Gateway",
                Status = "Healthy",
                StatusColor = "#4CAF50"
            });

            SystemStatus.Add(new SystemStatusViewModel
            {
                ServiceName = "User Service",
                Status = "Healthy",
                StatusColor = "#4CAF50"
            });

            SystemStatus.Add(new SystemStatusViewModel
            {
                ServiceName = "AI Service",
                Status = "Warning",
                StatusColor = "#FF9800"
            });

            SystemStatus.Add(new SystemStatusViewModel
            {
                ServiceName = "Trading Service",
                Status = "Healthy",
                StatusColor = "#4CAF50"
            });

            SystemStatus.Add(new SystemStatusViewModel
            {
                ServiceName = "Payment Service",
                Status = "Healthy",
                StatusColor = "#4CAF50"
            });

            SystemStatus.Add(new SystemStatusViewModel
            {
                ServiceName = "Notification Service",
                Status = "Healthy",
                StatusColor = "#4CAF50"
            });
        }

        private async Task LoadRecentAlertsAsync()
        {
            RecentAlerts.Clear();

            try
            {
                var alerts = await _notificationService.GetRecentAlertsAsync(5);
                
                foreach (var alert in alerts)
                {
                    RecentAlerts.Add(new AlertViewModel
                    {
                        Title = alert.Title,
                        Message = alert.Message,
                        Timestamp = alert.Timestamp
                    });
                }
            }
            catch (Exception ex)
            {
                // Fallback to mock data
                LoadMockRecentAlerts();
                System.Diagnostics.Debug.WriteLine($"Error loading recent alerts: {ex.Message}");
            }
        }

        private void LoadMockRecentAlerts()
        {
            RecentAlerts.Add(new AlertViewModel
            {
                Title = "High AI Usage",
                Message = "AI service usage has increased by 25% in the last hour",
                Timestamp = DateTime.Now.AddMinutes(-30)
            });

            RecentAlerts.Add(new AlertViewModel
            {
                Title = "Cost Threshold Reached",
                Message = "Monthly AI cost has reached 80% of budget",
                Timestamp = DateTime.Now.AddHours(-2)
            });

            RecentAlerts.Add(new AlertViewModel
            {
                Title = "New User Registration",
                Message = "5 new users have registered in the last 24 hours",
                Timestamp = DateTime.Now.AddHours(-4)
            });
        }

        private string GetStatusColor(ServiceStatus status)
        {
            return status switch
            {
                ServiceStatus.Healthy => "#4CAF50",
                ServiceStatus.Warning => "#FF9800",
                ServiceStatus.Critical => "#F44336",
                ServiceStatus.Offline => "#9E9E9E",
                _ => "#666666"
            };
        }

        private void NavigateToAlerts()
        {
            _navigationService.NavigateTo<SystemHealthViewModel>();
        }

        private void NavigateToUsers()
        {
            _navigationService.NavigateTo<UserManagementViewModel>();
        }

        private void NavigateToAnalytics()
        {
            _navigationService.NavigateTo<AiAnalyticsViewModel>();
        }

        private void NavigateToHealth()
        {
            _navigationService.NavigateTo<SystemHealthViewModel>();
        }

        private void NavigateToSettings()
        {
            _navigationService.NavigateTo<SettingsViewModel>();
        }
    }

    public class KpiCardViewModel
    {
        public string Title { get; set; } = "";
        public string Value { get; set; } = "";
        public string Icon { get; set; } = "";
        public string ChangeText { get; set; } = "";
        public string ChangeValue { get; set; } = "";
        public string ChangeColor { get; set; } = "#333";
    }

    public class SystemStatusViewModel
    {
        public string ServiceName { get; set; } = "";
        public string Status { get; set; } = "";
        public string StatusColor { get; set; } = "#666";
    }

    public class AlertViewModel
    {
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object? parameter) => _execute();
    }
} 
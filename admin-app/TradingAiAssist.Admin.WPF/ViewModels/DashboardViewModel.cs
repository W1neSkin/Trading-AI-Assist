using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TradingAiAssist.Admin.Core.Models;
using System.Collections.Generic; // Added for EqualityComparer
using System; // Added for DateTime
using System.Threading.Tasks; // Added for Task

namespace TradingAiAssist.Admin.WPF.ViewModels
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        private DateTime _lastUpdated;
        private bool _isLoading;

        public DashboardViewModel()
        {
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

            // TODO: Replace with actual service calls
            // For now, using mock data
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

            // TODO: Replace with actual service calls
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

            // TODO: Replace with actual service calls
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

        private void NavigateToAlerts()
        {
            // TODO: Implement navigation to alerts view
        }

        private void NavigateToUsers()
        {
            // TODO: Implement navigation to users view
        }

        private void NavigateToAnalytics()
        {
            // TODO: Implement navigation to analytics view
        }

        private void NavigateToHealth()
        {
            // TODO: Implement navigation to health view
        }

        private void NavigateToSettings()
        {
            // TODO: Implement navigation to settings view
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion
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
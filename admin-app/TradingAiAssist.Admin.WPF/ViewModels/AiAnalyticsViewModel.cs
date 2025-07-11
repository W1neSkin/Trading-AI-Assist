using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TradingAiAssist.Admin.Core.Models;
using TradingAiAssist.Admin.Core.Services;

namespace TradingAiAssist.Admin.WPF.ViewModels
{
    public class AiAnalyticsViewModel : INotifyPropertyChanged
    {
        private readonly IAiAnalyticsService _analyticsService;
        private TimeRange _selectedTimeRange;
        private bool _isLoading;

        public AiAnalyticsViewModel(IAiAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
            
            KeyMetrics = new ObservableCollection<MetricViewModel>();
            TopUsers = new ObservableCollection<TopUserViewModel>();
            ModelUsage = new ObservableCollection<ModelUsageViewModel>();
            CostAlerts = new ObservableCollection<CostAlertViewModel>();
            TimeRanges = new ObservableCollection<TimeRange>
            {
                new TimeRange { DisplayName = "Last 24 Hours", Value = "24h" },
                new TimeRange { DisplayName = "Last 7 Days", Value = "7d" },
                new TimeRange { DisplayName = "Last 30 Days", Value = "30d" },
                new TimeRange { DisplayName = "Last 90 Days", Value = "90d" }
            };

            SelectedTimeRange = TimeRanges[1]; // Default to 7 days

            // Initialize commands
            RefreshCommand = new RelayCommand(async () => await RefreshAnalyticsAsync());
            ExportCommand = new RelayCommand(async () => await ExportReportAsync());
            ViewAllAlertsCommand = new RelayCommand(() => ViewAllAlerts());
            SetCostLimitsCommand = new RelayCommand(() => SetCostLimits());
            GenerateReportCommand = new RelayCommand(async () => await GenerateReportAsync());
            UsageSettingsCommand = new RelayCommand(() => UsageSettings());

            // Initialize data
            _ = LoadAnalyticsAsync();
        }

        public TimeRange SelectedTimeRange
        {
            get => _selectedTimeRange;
            set
            {
                SetProperty(ref _selectedTimeRange, value);
                _ = LoadAnalyticsAsync();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ObservableCollection<MetricViewModel> KeyMetrics { get; }
        public ObservableCollection<TopUserViewModel> TopUsers { get; }
        public ObservableCollection<ModelUsageViewModel> ModelUsage { get; }
        public ObservableCollection<CostAlertViewModel> CostAlerts { get; }
        public ObservableCollection<TimeRange> TimeRanges { get; }

        public ICommand RefreshCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand ViewAllAlertsCommand { get; }
        public ICommand SetCostLimitsCommand { get; }
        public ICommand GenerateReportCommand { get; }
        public ICommand UsageSettingsCommand { get; }

        private async Task LoadAnalyticsAsync()
        {
            IsLoading = true;
            try
            {
                await Task.WhenAll(
                    LoadKeyMetricsAsync(),
                    LoadTopUsersAsync(),
                    LoadModelUsageAsync(),
                    LoadCostAlertsAsync()
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading analytics: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RefreshAnalyticsAsync()
        {
            await LoadAnalyticsAsync();
        }

        private async Task LoadKeyMetricsAsync()
        {
            try
            {
                var report = await _analyticsService.GetUsageReportAsync(new AiUsageReportRequest
                {
                    TimeRange = SelectedTimeRange.Value
                });

                KeyMetrics.Clear();

                KeyMetrics.Add(new MetricViewModel
                {
                    Title = "Total Requests",
                    Value = report.TotalRequests.ToString("N0"),
                    Icon = "📊",
                    Color = "#2196F3",
                    ChangeText = report.RequestsChange > 0 ? "+" : "",
                    ChangeValue = $"{report.RequestsChange:F1}%",
                    ChangeColor = report.RequestsChange >= 0 ? "#4CAF50" : "#F44336"
                });

                KeyMetrics.Add(new MetricViewModel
                {
                    Title = "Total Cost",
                    Value = report.TotalCost.ToString("C"),
                    Icon = "💰",
                    Color = "#FF9800",
                    ChangeText = report.CostChange > 0 ? "+" : "",
                    ChangeValue = $"{report.CostChange:F1}%",
                    ChangeColor = report.CostChange <= 0 ? "#4CAF50" : "#F44336"
                });

                KeyMetrics.Add(new MetricViewModel
                {
                    Title = "Active Users",
                    Value = report.ActiveUsers.ToString(),
                    Icon = "👥",
                    Color = "#4CAF50",
                    ChangeText = report.ActiveUsersChange > 0 ? "+" : "",
                    ChangeValue = $"{report.ActiveUsersChange:F1}%",
                    ChangeColor = report.ActiveUsersChange >= 0 ? "#4CAF50" : "#F44336"
                });

                KeyMetrics.Add(new MetricViewModel
                {
                    Title = "Avg Response Time",
                    Value = $"{report.AverageResponseTime:F1}ms",
                    Icon = "⚡",
                    Color = "#9C27B0",
                    ChangeText = report.ResponseTimeChange < 0 ? "+" : "",
                    ChangeValue = $"{Math.Abs(report.ResponseTimeChange):F1}%",
                    ChangeColor = report.ResponseTimeChange <= 0 ? "#4CAF50" : "#F44336"
                });

                KeyMetrics.Add(new MetricViewModel
                {
                    Title = "Success Rate",
                    Value = $"{report.SuccessRate:F1}%",
                    Icon = "✅",
                    Color = "#4CAF50",
                    ChangeText = report.SuccessRateChange > 0 ? "+" : "",
                    ChangeValue = $"{report.SuccessRateChange:F1}%",
                    ChangeColor = report.SuccessRateChange >= 0 ? "#4CAF50" : "#F44336"
                });

                KeyMetrics.Add(new MetricViewModel
                {
                    Title = "Error Rate",
                    Value = $"{report.ErrorRate:F1}%",
                    Icon = "❌",
                    Color = "#F44336",
                    ChangeText = report.ErrorRateChange < 0 ? "+" : "",
                    ChangeValue = $"{Math.Abs(report.ErrorRateChange):F1}%",
                    ChangeColor = report.ErrorRateChange <= 0 ? "#4CAF50" : "#F44336"
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading key metrics: {ex.Message}");
            }
        }

        private async Task LoadTopUsersAsync()
        {
            try
            {
                var topUsers = await _analyticsService.GetTopUsersAsync(new TopUsersRequest
                {
                    TimeRange = SelectedTimeRange.Value,
                    Limit = 10
                });

                TopUsers.Clear();
                foreach (var user in topUsers)
                {
                    TopUsers.Add(new TopUserViewModel
                    {
                        UserName = user.UserName,
                        RequestCount = user.RequestCount,
                        TotalCost = user.TotalCost,
                        LastUsed = user.LastUsed
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading top users: {ex.Message}");
            }
        }

        private async Task LoadModelUsageAsync()
        {
            try
            {
                var modelUsage = await _analyticsService.GetModelUsageAsync(new ModelUsageRequest
                {
                    TimeRange = SelectedTimeRange.Value
                });

                ModelUsage.Clear();
                foreach (var model in modelUsage)
                {
                    ModelUsage.Add(new ModelUsageViewModel
                    {
                        ModelName = model.ModelName,
                        RequestCount = model.RequestCount,
                        UsagePercentage = model.UsagePercentage
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading model usage: {ex.Message}");
            }
        }

        private async Task LoadCostAlertsAsync()
        {
            try
            {
                var alerts = await _analyticsService.GetCostAlertsAsync(new CostAlertsRequest
                {
                    TimeRange = SelectedTimeRange.Value,
                    Limit = 5
                });

                CostAlerts.Clear();
                foreach (var alert in alerts)
                {
                    CostAlerts.Add(new CostAlertViewModel
                    {
                        Title = alert.Title,
                        Message = alert.Message,
                        Timestamp = alert.Timestamp
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading cost alerts: {ex.Message}");
            }
        }

        private async Task ExportReportAsync()
        {
            try
            {
                // TODO: Implement export functionality
                System.Diagnostics.Debug.WriteLine("Export report clicked");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error exporting report: {ex.Message}");
            }
        }

        private void ViewAllAlerts()
        {
            // TODO: Navigate to alerts view
            System.Diagnostics.Debug.WriteLine("View all alerts clicked");
        }

        private void SetCostLimits()
        {
            // TODO: Open cost limits dialog
            System.Diagnostics.Debug.WriteLine("Set cost limits clicked");
        }

        private async Task GenerateReportAsync()
        {
            try
            {
                // TODO: Generate detailed report
                System.Diagnostics.Debug.WriteLine("Generate report clicked");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error generating report: {ex.Message}");
            }
        }

        private void UsageSettings()
        {
            // TODO: Open usage settings dialog
            System.Diagnostics.Debug.WriteLine("Usage settings clicked");
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

    public class MetricViewModel
    {
        public string Title { get; set; } = "";
        public string Value { get; set; } = "";
        public string Icon { get; set; } = "";
        public string Color { get; set; } = "#333";
        public string ChangeText { get; set; } = "";
        public string ChangeValue { get; set; } = "";
        public string ChangeColor { get; set; } = "#333";
    }

    public class TopUserViewModel
    {
        public string UserName { get; set; } = "";
        public int RequestCount { get; set; }
        public decimal TotalCost { get; set; }
        public DateTime LastUsed { get; set; }
    }

    public class ModelUsageViewModel
    {
        public string ModelName { get; set; } = "";
        public int RequestCount { get; set; }
        public double UsagePercentage { get; set; }
    }

    public class CostAlertViewModel
    {
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }

    public class TimeRange
    {
        public string DisplayName { get; set; } = "";
        public string Value { get; set; } = "";
    }
} 
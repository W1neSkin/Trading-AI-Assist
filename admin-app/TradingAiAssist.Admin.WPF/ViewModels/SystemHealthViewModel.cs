using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TradingAiAssist.Admin.Core.Models;
using TradingAiAssist.Admin.Core.Services;

namespace TradingAiAssist.Admin.WPF.ViewModels
{
    public class SystemHealthViewModel : INotifyPropertyChanged
    {
        private readonly ISystemHealthService _healthService;
        private DateTime _lastUpdated;
        private bool _isLoading;

        public SystemHealthViewModel(ISystemHealthService healthService)
        {
            _healthService = healthService;
            
            SystemStatus = new ObservableCollection<ServiceStatusViewModel>();
            PerformanceMetrics = new ObservableCollection<PerformanceMetricViewModel>();
            RecentAlerts = new ObservableCollection<SystemAlertViewModel>();
            MaintenanceSchedule = new ObservableCollection<MaintenanceItemViewModel>();

            // Initialize commands
            RefreshCommand = new RelayCommand(async () => await RefreshHealthAsync());
            ExportCommand = new RelayCommand(async () => await ExportReportAsync());
            RestartServiceCommand = new RelayCommand(async () => await RestartServiceAsync());
            ViewLogsCommand = new RelayCommand(() => ViewLogs());
            HealthCheckCommand = new RelayCommand(async () => await PerformHealthCheckAsync());
            SettingsCommand = new RelayCommand(() => OpenSettings());

            // Initialize data
            _ = LoadHealthDataAsync();
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

        public string OverallHealth { get; private set; } = "Healthy";
        public string OverallHealthColor { get; private set; } = "#4CAF50";
        public string SystemUptime { get; private set; } = "99.8%";
        public int ActiveServices { get; private set; } = 6;
        public long TotalRequests { get; private set; } = 125847;
        public double AverageResponseTime { get; private set; } = 245.5;

        public ObservableCollection<ServiceStatusViewModel> SystemStatus { get; }
        public ObservableCollection<PerformanceMetricViewModel> PerformanceMetrics { get; }
        public ObservableCollection<SystemAlertViewModel> RecentAlerts { get; }
        public ObservableCollection<MaintenanceItemViewModel> MaintenanceSchedule { get; }

        public ICommand RefreshCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand RestartServiceCommand { get; }
        public ICommand ViewLogsCommand { get; }
        public ICommand HealthCheckCommand { get; }
        public ICommand SettingsCommand { get; }

        private async Task LoadHealthDataAsync()
        {
            IsLoading = true;
            try
            {
                await Task.WhenAll(
                    LoadSystemStatusAsync(),
                    LoadPerformanceMetricsAsync(),
                    LoadRecentAlertsAsync(),
                    LoadMaintenanceScheduleAsync()
                );
                LastUpdated = DateTime.Now;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading health data: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RefreshHealthAsync()
        {
            await LoadHealthDataAsync();
        }

        private async Task LoadSystemStatusAsync()
        {
            try
            {
                var healthStatus = await _healthService.GetSystemHealthAsync();

                SystemStatus.Clear();
                foreach (var service in healthStatus.Services)
                {
                    SystemStatus.Add(new ServiceStatusViewModel
                    {
                        ServiceName = service.ServiceName,
                        Status = service.Status,
                        StatusColor = GetStatusColor(service.Status),
                        Uptime = service.Uptime,
                        ResponseTime = $"{service.ResponseTime:F0}ms"
                    });
                }

                // Update overall health
                var healthyServices = healthStatus.Services.Count(s => s.Status == "Healthy");
                var totalServices = healthStatus.Services.Count;
                
                if (healthyServices == totalServices)
                {
                    OverallHealth = "Healthy";
                    OverallHealthColor = "#4CAF50";
                }
                else if (healthyServices >= totalServices * 0.8)
                {
                    OverallHealth = "Warning";
                    OverallHealthColor = "#FF9800";
                }
                else
                {
                    OverallHealth = "Critical";
                    OverallHealthColor = "#F44336";
                }

                ActiveServices = healthyServices;
                OnPropertyChanged(nameof(OverallHealth));
                OnPropertyChanged(nameof(OverallHealthColor));
                OnPropertyChanged(nameof(ActiveServices));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading system status: {ex.Message}");
            }
        }

        private async Task LoadPerformanceMetricsAsync()
        {
            try
            {
                var metrics = await _healthService.GetPerformanceMetricsAsync();

                PerformanceMetrics.Clear();
                foreach (var metric in metrics)
                {
                    PerformanceMetrics.Add(new PerformanceMetricViewModel
                    {
                        ServiceName = metric.ServiceName,
                        CpuUsage = metric.CpuUsage,
                        MemoryUsage = metric.MemoryUsage,
                        ResponseTime = metric.ResponseTime,
                        RequestsPerMinute = metric.RequestsPerMinute,
                        ErrorRate = metric.ErrorRate,
                        Status = metric.Status
                    });
                }

                // Update summary metrics
                if (metrics.Any())
                {
                    TotalRequests = metrics.Sum(m => m.RequestsPerMinute) * 60; // Estimate total requests
                    AverageResponseTime = metrics.Average(m => m.ResponseTime);
                    OnPropertyChanged(nameof(TotalRequests));
                    OnPropertyChanged(nameof(AverageResponseTime));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading performance metrics: {ex.Message}");
            }
        }

        private async Task LoadRecentAlertsAsync()
        {
            try
            {
                var alerts = await _healthService.GetRecentAlertsAsync(new AlertsRequest
                {
                    Limit = 10
                });

                RecentAlerts.Clear();
                foreach (var alert in alerts)
                {
                    RecentAlerts.Add(new SystemAlertViewModel
                    {
                        Title = alert.Title,
                        Message = alert.Message,
                        Timestamp = alert.Timestamp,
                        AlertColor = GetAlertColor(alert.Severity),
                        AlertBackground = GetAlertBackground(alert.Severity)
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading recent alerts: {ex.Message}");
            }
        }

        private async Task LoadMaintenanceScheduleAsync()
        {
            try
            {
                var schedule = await _healthService.GetMaintenanceScheduleAsync();

                MaintenanceSchedule.Clear();
                foreach (var item in schedule)
                {
                    MaintenanceSchedule.Add(new MaintenanceItemViewModel
                    {
                        ServiceName = item.ServiceName,
                        Description = item.Description,
                        ScheduledTime = item.ScheduledTime
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading maintenance schedule: {ex.Message}");
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

        private async Task RestartServiceAsync()
        {
            try
            {
                // TODO: Implement service restart functionality
                System.Diagnostics.Debug.WriteLine("Restart service clicked");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error restarting service: {ex.Message}");
            }
        }

        private void ViewLogs()
        {
            // TODO: Open logs viewer
            System.Diagnostics.Debug.WriteLine("View logs clicked");
        }

        private async Task PerformHealthCheckAsync()
        {
            try
            {
                await LoadHealthDataAsync();
                System.Diagnostics.Debug.WriteLine("Health check performed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error performing health check: {ex.Message}");
            }
        }

        private void OpenSettings()
        {
            // TODO: Open system health settings
            System.Diagnostics.Debug.WriteLine("Open settings clicked");
        }

        private string GetStatusColor(string status)
        {
            return status.ToLower() switch
            {
                "healthy" => "#4CAF50",
                "warning" => "#FF9800",
                "critical" => "#F44336",
                "offline" => "#9E9E9E",
                _ => "#666666"
            };
        }

        private string GetAlertColor(string severity)
        {
            return severity.ToLower() switch
            {
                "critical" => "#F44336",
                "warning" => "#FF9800",
                "info" => "#2196F3",
                _ => "#666666"
            };
        }

        private string GetAlertBackground(string severity)
        {
            return severity.ToLower() switch
            {
                "critical" => "#FFEBEE",
                "warning" => "#FFF3E0",
                "info" => "#E3F2FD",
                _ => "#F5F5F5"
            };
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

    public class ServiceStatusViewModel
    {
        public string ServiceName { get; set; } = "";
        public string Status { get; set; } = "";
        public string StatusColor { get; set; } = "#666";
        public string Uptime { get; set; } = "";
        public string ResponseTime { get; set; } = "";
    }

    public class PerformanceMetricViewModel
    {
        public string ServiceName { get; set; } = "";
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public double ResponseTime { get; set; }
        public int RequestsPerMinute { get; set; }
        public double ErrorRate { get; set; }
        public string Status { get; set; } = "";
    }

    public class SystemAlertViewModel
    {
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public string AlertColor { get; set; } = "#666";
        public string AlertBackground { get; set; } = "#F5F5F5";
    }

    public class MaintenanceItemViewModel
    {
        public string ServiceName { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime ScheduledTime { get; set; }
    }
} 
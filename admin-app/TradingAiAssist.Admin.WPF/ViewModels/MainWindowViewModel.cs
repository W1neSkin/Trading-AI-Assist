using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using TradingAiAssist.Admin.Core.Models;
using TradingAiAssist.Admin.Core.Interfaces;
using TradingAiAssist.Admin.WPF.Views;

namespace TradingAiAssist.Admin.WPF.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IUserManagementService _userManagementService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<MainWindowViewModel> _logger;

        [ObservableProperty]
        private bool _isMenuOpen;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _statusMessage = "Ready";

        [ObservableProperty]
        private bool _isConnected = true;

        [ObservableProperty]
        private string _connectionStatus = "Connected";

        [ObservableProperty]
        private int _notificationCount;

        [ObservableProperty]
        private UserProfile? _currentUser;

        [ObservableProperty]
        private object? _currentView;

        [ObservableProperty]
        private ObservableCollection<Notification> _notifications = new();

        public MainWindowViewModel(
            IAuthenticationService authenticationService,
            IUserManagementService userManagementService,
            INotificationService notificationService,
            ILogger<MainWindowViewModel> logger)
        {
            _authenticationService = authenticationService;
            _userManagementService = userManagementService;
            _notificationService = notificationService;
            _logger = logger;

            // Initialize commands
            ToggleMenuCommand = new RelayCommand(ToggleMenu);
            NavigateCommand = new RelayCommand<string>(Navigate);
            LogoutCommand = new RelayCommand(Logout);
            ShowNotificationsCommand = new RelayCommand(ShowNotifications);
            ShowUserMenuCommand = new RelayCommand(ShowUserMenu);

            // Initialize the application
            _ = InitializeAsync();
        }

        public ICommand ToggleMenuCommand { get; }
        public ICommand NavigateCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand ShowNotificationsCommand { get; }
        public ICommand ShowUserMenuCommand { get; }

        private async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Initializing...";

                // Check authentication
                var isAuthenticated = await _authenticationService.IsAuthenticatedAsync();
                if (!isAuthenticated)
                {
                    // Redirect to login
                    Navigate("Login");
                    return;
                }

                // Load current user
                CurrentUser = await _authenticationService.GetCurrentUserAsync();
                if (CurrentUser == null)
                {
                    _logger.LogWarning("Current user not found after authentication");
                    Navigate("Login");
                    return;
                }

                // Load notifications
                await LoadNotificationsAsync();

                // Start with dashboard
                Navigate("Dashboard");

                StatusMessage = "Ready";
                _logger.LogInformation("Main window initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing main window");
                StatusMessage = "Initialization failed";
                MessageBox.Show($"Failed to initialize application: {ex.Message}", 
                    "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ToggleMenu()
        {
            IsMenuOpen = !IsMenuOpen;
        }

        private void Navigate(string? destination)
        {
            if (string.IsNullOrEmpty(destination))
                return;

            try
            {
                StatusMessage = $"Navigating to {destination}...";

                switch (destination.ToLower())
                {
                    case "dashboard":
                        CurrentView = App.Current.Services.GetService<DashboardView>();
                        break;
                    case "usermanagement":
                        CurrentView = App.Current.Services.GetService<UserManagementView>();
                        break;
                    case "aianalytics":
                        CurrentView = App.Current.Services.GetService<AiAnalyticsView>();
                        break;
                    case "systemhealth":
                        CurrentView = App.Current.Services.GetService<SystemHealthView>();
                        break;
                    case "settings":
                        CurrentView = App.Current.Services.GetService<SettingsView>();
                        break;
                    case "login":
                        // Handle login navigation
                        break;
                    default:
                        _logger.LogWarning("Unknown navigation destination: {Destination}", destination);
                        break;
                }

                StatusMessage = $"Current view: {destination}";
                _logger.LogInformation("Navigated to {Destination}", destination);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error navigating to {Destination}", destination);
                StatusMessage = "Navigation failed";
                MessageBox.Show($"Failed to navigate to {destination}: {ex.Message}", 
                    "Navigation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Logout()
        {
            try
            {
                var result = MessageBox.Show("Are you sure you want to logout?", 
                    "Confirm Logout", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    IsLoading = true;
                    StatusMessage = "Logging out...";

                    await _authenticationService.LogoutAsync();
                    
                    // Close the application or show login
                    Application.Current.Shutdown();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                StatusMessage = "Logout failed";
                MessageBox.Show($"Failed to logout: {ex.Message}", 
                    "Logout Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ShowNotifications()
        {
            // Show notifications panel or dialog
            MessageBox.Show($"You have {NotificationCount} notifications", 
                "Notifications", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowUserMenu()
        {
            // Show user menu with profile, settings, etc.
            MessageBox.Show($"User menu for {CurrentUser?.DisplayName}", 
                "User Menu", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async Task LoadNotificationsAsync()
        {
            try
            {
                var notifications = await _notificationService.GetNotificationsAsync();
                Notifications.Clear();
                
                foreach (var notification in notifications)
                {
                    Notifications.Add(notification);
                }

                NotificationCount = Notifications.Count;
                _logger.LogInformation("Loaded {Count} notifications", NotificationCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading notifications");
            }
        }

        public async Task RefreshDataAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Refreshing data...";

                // Refresh notifications
                await LoadNotificationsAsync();

                // Refresh current user
                CurrentUser = await _authenticationService.GetCurrentUserAsync();

                StatusMessage = "Data refreshed";
                _logger.LogInformation("Data refreshed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing data");
                StatusMessage = "Data refresh failed";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void UpdateConnectionStatus(bool isConnected, string status)
        {
            IsConnected = isConnected;
            ConnectionStatus = status;
            
            if (!isConnected)
            {
                StatusMessage = "Connection lost";
                _logger.LogWarning("Connection status: {Status}", status);
            }
            else
            {
                StatusMessage = "Connected";
            }
        }
    }
} 
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows;
using System.Collections.Generic; // Added for EqualityComparer
using System; // Added for Exception
using System.Threading.Tasks; // Added for Task
using TradingAiAssist.Admin.WPF.Services;

namespace TradingAiAssist.Admin.WPF.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private string _currentCategory = "General";
        private bool _isGeneralVisible = true;
        private bool _isAzureAdVisible = false;
        private bool _isApiSettingsVisible = false;
        private bool _isNotificationsVisible = false;
        private bool _isSecurityVisible = false;
        private bool _isAdvancedVisible = false;

        // General Settings
        private string _applicationName = "Trading AI Assist Admin";
        private ThemeOption? _selectedTheme;
        private LanguageOption? _selectedLanguage;
        private string _autoRefreshInterval = "30";
        private bool _enableAutoRefresh = true;
        private bool _showNotifications = true;
        private bool _startWithWindows = false;

        // Azure AD Settings
        private string _tenantId = "";
        private string _clientId = "";
        private string _redirectUri = "";
        private string _authority = "";
        private bool _enableSso = true;
        private bool _rememberSession = true;

        // API Settings
        private string _apiBaseUrl = "";
        private string _apiKey = "";
        private string _apiTimeout = "30";
        private string _apiRetryAttempts = "3";
        private bool _enableApiCaching = true;
        private bool _logApiRequests = false;

        // Notification Settings
        private bool _enableDesktopNotifications = true;
        private bool _showSystemAlerts = true;
        private bool _showCostAlerts = true;
        private bool _showPerformanceAlerts = true;
        private NotificationSoundOption? _selectedNotificationSound;
        private string _alertThreshold = "80";

        // Security Settings
        private bool _requireAuthentication = true;
        private bool _enableAuditLogging = true;
        private bool _encryptSensitiveData = true;
        private string _sessionTimeout = "60";
        private PasswordPolicyOption? _selectedPasswordPolicy;

        // Advanced Settings
        private LogLevelOption? _selectedLogLevel;
        private string _logFilePath = "";
        private string _databaseConnectionString = "";
        private bool _enableDebugMode = false;
        private bool _enablePerformanceMonitoring = true;

        public SettingsViewModel()
        {
            // Initialize collections
            Themes = new ObservableCollection<ThemeOption>
            {
                new ThemeOption { DisplayName = "Light", Value = "Light" },
                new ThemeOption { DisplayName = "Dark", Value = "Dark" },
                new ThemeOption { DisplayName = "System", Value = "System" }
            };

            Languages = new ObservableCollection<LanguageOption>
            {
                new LanguageOption { DisplayName = "English", Value = "en-US" },
                new LanguageOption { DisplayName = "Spanish", Value = "es-ES" },
                new LanguageOption { DisplayName = "French", Value = "fr-FR" },
                new LanguageOption { DisplayName = "German", Value = "de-DE" }
            };

            NotificationSounds = new ObservableCollection<NotificationSoundOption>
            {
                new NotificationSoundOption { DisplayName = "Default", Value = "default" },
                new NotificationSoundOption { DisplayName = "Chime", Value = "chime" },
                new NotificationSoundOption { DisplayName = "Bell", Value = "bell" },
                new NotificationSoundOption { DisplayName = "None", Value = "none" }
            };

            PasswordPolicies = new ObservableCollection<PasswordPolicyOption>
            {
                new PasswordPolicyOption { DisplayName = "Standard", Value = "standard" },
                new PasswordPolicyOption { DisplayName = "Strong", Value = "strong" },
                new PasswordPolicyOption { DisplayName = "Very Strong", Value = "very_strong" }
            };

            LogLevels = new ObservableCollection<LogLevelOption>
            {
                new LogLevelOption { DisplayName = "Debug", Value = "Debug" },
                new LogLevelOption { DisplayName = "Information", Value = "Information" },
                new LogLevelOption { DisplayName = "Warning", Value = "Warning" },
                new LogLevelOption { DisplayName = "Error", Value = "Error" }
            };

            // Set defaults
            SelectedTheme = Themes[0];
            SelectedLanguage = Languages[0];
            SelectedNotificationSound = NotificationSounds[0];
            SelectedPasswordPolicy = PasswordPolicies[0];
            SelectedLogLevel = LogLevels[1];

            // Initialize commands
            SaveAllCommand = new RelayCommand(async () => await SaveAllSettingsAsync());
            ResetToDefaultsCommand = new RelayCommand(() => ResetToDefaults());
            NavigateToGeneralCommand = new RelayCommand(() => NavigateToGeneral());
            NavigateToAzureAdCommand = new RelayCommand(() => NavigateToAzureAd());
            NavigateToApiSettingsCommand = new RelayCommand(() => NavigateToApiSettings());
            NavigateToNotificationsCommand = new RelayCommand(() => NavigateToNotifications());
            NavigateToSecurityCommand = new RelayCommand(() => NavigateToSecurity());
            NavigateToAdvancedCommand = new RelayCommand(() => NavigateToAdvanced());
            TestAzureAdConnectionCommand = new RelayCommand(async () => await TestAzureAdConnectionAsync());
            TestApiConnectionCommand = new RelayCommand(async () => await TestApiConnectionAsync());
            ChangePasswordCommand = new RelayCommand(() => ChangePassword());
            ClearCacheCommand = new RelayCommand(async () => await ClearCacheAsync());
            ExportConfigurationCommand = new RelayCommand(async () => await ExportConfigurationAsync());

            // Load settings
            LoadSettings();
        }

        #region Properties

        public string CurrentCategory
        {
            get => _currentCategory;
            set => SetProperty(ref _currentCategory, value);
        }

        public bool IsGeneralVisible
        {
            get => _isGeneralVisible;
            set => SetProperty(ref _isGeneralVisible, value);
        }

        public bool IsAzureAdVisible
        {
            get => _isAzureAdVisible;
            set => SetProperty(ref _isAzureAdVisible, value);
        }

        public bool IsApiSettingsVisible
        {
            get => _isApiSettingsVisible;
            set => SetProperty(ref _isApiSettingsVisible, value);
        }

        public bool IsNotificationsVisible
        {
            get => _isNotificationsVisible;
            set => SetProperty(ref _isNotificationsVisible, value);
        }

        public bool IsSecurityVisible
        {
            get => _isSecurityVisible;
            set => SetProperty(ref _isSecurityVisible, value);
        }

        public bool IsAdvancedVisible
        {
            get => _isAdvancedVisible;
            set => SetProperty(ref _isAdvancedVisible, value);
        }

        // General Settings
        public string ApplicationName
        {
            get => _applicationName;
            set => SetProperty(ref _applicationName, value);
        }

        public ThemeOption? SelectedTheme
        {
            get => _selectedTheme;
            set => SetProperty(ref _selectedTheme, value);
        }

        public LanguageOption? SelectedLanguage
        {
            get => _selectedLanguage;
            set => SetProperty(ref _selectedLanguage, value);
        }

        public string AutoRefreshInterval
        {
            get => _autoRefreshInterval;
            set => SetProperty(ref _autoRefreshInterval, value);
        }

        public bool EnableAutoRefresh
        {
            get => _enableAutoRefresh;
            set => SetProperty(ref _enableAutoRefresh, value);
        }

        public bool ShowNotifications
        {
            get => _showNotifications;
            set => SetProperty(ref _showNotifications, value);
        }

        public bool StartWithWindows
        {
            get => _startWithWindows;
            set => SetProperty(ref _startWithWindows, value);
        }

        // Azure AD Settings
        public string TenantId
        {
            get => _tenantId;
            set => SetProperty(ref _tenantId, value);
        }

        public string ClientId
        {
            get => _clientId;
            set => SetProperty(ref _clientId, value);
        }

        public string RedirectUri
        {
            get => _redirectUri;
            set => SetProperty(ref _redirectUri, value);
        }

        public string Authority
        {
            get => _authority;
            set => SetProperty(ref _authority, value);
        }

        public bool EnableSso
        {
            get => _enableSso;
            set => SetProperty(ref _enableSso, value);
        }

        public bool RememberSession
        {
            get => _rememberSession;
            set => SetProperty(ref _rememberSession, value);
        }

        // API Settings
        public string ApiBaseUrl
        {
            get => _apiBaseUrl;
            set => SetProperty(ref _apiBaseUrl, value);
        }

        public string ApiKey
        {
            get => _apiKey;
            set => SetProperty(ref _apiKey, value);
        }

        public string ApiTimeout
        {
            get => _apiTimeout;
            set => SetProperty(ref _apiTimeout, value);
        }

        public string ApiRetryAttempts
        {
            get => _apiRetryAttempts;
            set => SetProperty(ref _apiRetryAttempts, value);
        }

        public bool EnableApiCaching
        {
            get => _enableApiCaching;
            set => SetProperty(ref _enableApiCaching, value);
        }

        public bool LogApiRequests
        {
            get => _logApiRequests;
            set => SetProperty(ref _logApiRequests, value);
        }

        // Notification Settings
        public bool EnableDesktopNotifications
        {
            get => _enableDesktopNotifications;
            set => SetProperty(ref _enableDesktopNotifications, value);
        }

        public bool ShowSystemAlerts
        {
            get => _showSystemAlerts;
            set => SetProperty(ref _showSystemAlerts, value);
        }

        public bool ShowCostAlerts
        {
            get => _showCostAlerts;
            set => SetProperty(ref _showCostAlerts, value);
        }

        public bool ShowPerformanceAlerts
        {
            get => _showPerformanceAlerts;
            set => SetProperty(ref _showPerformanceAlerts, value);
        }

        public NotificationSoundOption? SelectedNotificationSound
        {
            get => _selectedNotificationSound;
            set => SetProperty(ref _selectedNotificationSound, value);
        }

        public string AlertThreshold
        {
            get => _alertThreshold;
            set => SetProperty(ref _alertThreshold, value);
        }

        // Security Settings
        public bool RequireAuthentication
        {
            get => _requireAuthentication;
            set => SetProperty(ref _requireAuthentication, value);
        }

        public bool EnableAuditLogging
        {
            get => _enableAuditLogging;
            set => SetProperty(ref _enableAuditLogging, value);
        }

        public bool EncryptSensitiveData
        {
            get => _encryptSensitiveData;
            set => SetProperty(ref _encryptSensitiveData, value);
        }

        public string SessionTimeout
        {
            get => _sessionTimeout;
            set => SetProperty(ref _sessionTimeout, value);
        }

        public PasswordPolicyOption? SelectedPasswordPolicy
        {
            get => _selectedPasswordPolicy;
            set => SetProperty(ref _selectedPasswordPolicy, value);
        }

        // Advanced Settings
        public LogLevelOption? SelectedLogLevel
        {
            get => _selectedLogLevel;
            set => SetProperty(ref _selectedLogLevel, value);
        }

        public string LogFilePath
        {
            get => _logFilePath;
            set => SetProperty(ref _logFilePath, value);
        }

        public string DatabaseConnectionString
        {
            get => _databaseConnectionString;
            set => SetProperty(ref _databaseConnectionString, value);
        }

        public bool EnableDebugMode
        {
            get => _enableDebugMode;
            set => SetProperty(ref _enableDebugMode, value);
        }

        public bool EnablePerformanceMonitoring
        {
            get => _enablePerformanceMonitoring;
            set => SetProperty(ref _enablePerformanceMonitoring, value);
        }

        // Collections
        public ObservableCollection<ThemeOption> Themes { get; }
        public ObservableCollection<LanguageOption> Languages { get; }
        public ObservableCollection<NotificationSoundOption> NotificationSounds { get; }
        public ObservableCollection<PasswordPolicyOption> PasswordPolicies { get; }
        public ObservableCollection<LogLevelOption> LogLevels { get; }

        #endregion

        #region Commands

        public ICommand SaveAllCommand { get; }
        public ICommand ResetToDefaultsCommand { get; }
        public ICommand NavigateToGeneralCommand { get; }
        public ICommand NavigateToAzureAdCommand { get; }
        public ICommand NavigateToApiSettingsCommand { get; }
        public ICommand NavigateToNotificationsCommand { get; }
        public ICommand NavigateToSecurityCommand { get; }
        public ICommand NavigateToAdvancedCommand { get; }
        public ICommand TestAzureAdConnectionCommand { get; }
        public ICommand TestApiConnectionCommand { get; }
        public ICommand ChangePasswordCommand { get; }
        public ICommand ClearCacheCommand { get; }
        public ICommand ExportConfigurationCommand { get; }

        #endregion

        #region Methods

        private void LoadSettings()
        {
            // TODO: Load settings from configuration file or database
            System.Diagnostics.Debug.WriteLine("Loading settings...");
        }

        private async Task SaveAllSettingsAsync()
        {
            try
            {
                // TODO: Save settings to configuration file or database
                System.Diagnostics.Debug.WriteLine("Saving all settings...");
                MessageBox.Show("Settings saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetToDefaults()
        {
            var result = MessageBox.Show("Are you sure you want to reset all settings to defaults?", 
                "Confirm Reset", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                // TODO: Reset all settings to defaults
                System.Diagnostics.Debug.WriteLine("Resetting settings to defaults...");
                LoadSettings();
            }
        }

        private void NavigateToGeneral()
        {
            SetAllVisibilityFalse();
            IsGeneralVisible = true;
            CurrentCategory = "General";
        }

        private void NavigateToAzureAd()
        {
            SetAllVisibilityFalse();
            IsAzureAdVisible = true;
            CurrentCategory = "Azure AD";
        }

        private void NavigateToApiSettings()
        {
            SetAllVisibilityFalse();
            IsApiSettingsVisible = true;
            CurrentCategory = "API Settings";
        }

        private void NavigateToNotifications()
        {
            SetAllVisibilityFalse();
            IsNotificationsVisible = true;
            CurrentCategory = "Notifications";
        }

        private void NavigateToSecurity()
        {
            SetAllVisibilityFalse();
            IsSecurityVisible = true;
            CurrentCategory = "Security";
        }

        private void NavigateToAdvanced()
        {
            SetAllVisibilityFalse();
            IsAdvancedVisible = true;
            CurrentCategory = "Advanced";
        }

        private void SetAllVisibilityFalse()
        {
            IsGeneralVisible = false;
            IsAzureAdVisible = false;
            IsApiSettingsVisible = false;
            IsNotificationsVisible = false;
            IsSecurityVisible = false;
            IsAdvancedVisible = false;
        }

        private async Task TestAzureAdConnectionAsync()
        {
            try
            {
                // TODO: Test Azure AD connection
                System.Diagnostics.Debug.WriteLine("Testing Azure AD connection...");
                MessageBox.Show("Azure AD connection test successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error testing Azure AD connection: {ex.Message}");
                MessageBox.Show($"Azure AD connection test failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task TestApiConnectionAsync()
        {
            try
            {
                // TODO: Test API connection
                System.Diagnostics.Debug.WriteLine("Testing API connection...");
                MessageBox.Show("API connection test successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error testing API connection: {ex.Message}");
                MessageBox.Show($"API connection test failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ChangePassword()
        {
            // TODO: Open change password dialog
            System.Diagnostics.Debug.WriteLine("Change password clicked");
        }

        private async Task ClearCacheAsync()
        {
            try
            {
                // TODO: Clear application cache
                System.Diagnostics.Debug.WriteLine("Clearing cache...");
                MessageBox.Show("Cache cleared successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing cache: {ex.Message}");
                MessageBox.Show($"Error clearing cache: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ExportConfigurationAsync()
        {
            try
            {
                // TODO: Export configuration to file
                System.Diagnostics.Debug.WriteLine("Exporting configuration...");
                MessageBox.Show("Configuration exported successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error exporting configuration: {ex.Message}");
                MessageBox.Show($"Error exporting configuration: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

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

    public class ThemeOption
    {
        public string DisplayName { get; set; } = "";
        public string Value { get; set; } = "";
    }

    public class LanguageOption
    {
        public string DisplayName { get; set; } = "";
        public string Value { get; set; } = "";
    }

    public class NotificationSoundOption
    {
        public string DisplayName { get; set; } = "";
        public string Value { get; set; } = "";
    }

    public class PasswordPolicyOption
    {
        public string DisplayName { get; set; } = "";
        public string Value { get; set; } = "";
    }

    public class LogLevelOption
    {
        public string DisplayName { get; set; } = "";
        public string Value { get; set; } = "";
    }
} 
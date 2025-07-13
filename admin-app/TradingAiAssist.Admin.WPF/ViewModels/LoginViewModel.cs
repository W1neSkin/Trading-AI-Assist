using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Windows;
using System.Linq;
using TradingAiAssist.Admin.Core.Interfaces;
using TradingAiAssist.Admin.Core.Models;
using TradingAiAssist.Admin.WPF.Services;
using TradingAiAssist.Admin.WPF.Views;

namespace TradingAiAssist.Admin.WPF.ViewModels
{
    /// <summary>
    /// ViewModel for the login screen with Azure AD authentication
    /// </summary>
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly INavigationService _navigationService;
        private readonly ILogger<LoginViewModel> _logger;
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private string _email = "";

        [ObservableProperty]
        private string _password = "";

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private string _statusMessage = "";

        [ObservableProperty]
        private bool _isError = false;

        public LoginViewModel(
            IAuthenticationService authenticationService,
            INavigationService navigationService,
            ILogger<LoginViewModel> logger,
            IServiceProvider serviceProvider)
        {
            _authenticationService = authenticationService;
            _navigationService = navigationService;
            _logger = logger;
            _serviceProvider = serviceProvider;

            // Set initial status message
            StatusMessage = "Please sign in with your Azure AD credentials";
        }

        /// <summary>
        /// Command to handle Azure AD authentication
        /// </summary>
        [RelayCommand]
        private async Task SignInAsync()
        {
            try
            {
                IsLoading = true;
                IsError = false;
                StatusMessage = "Authenticating with Azure AD...";

                _logger.LogInformation("Starting Azure AD authentication");

                // Perform Azure AD authentication
                var authResult = await _authenticationService.AuthenticateAsync();

                if (authResult.IsSuccess)
                {
                    _logger.LogInformation("Authentication successful for user: {User}", authResult.UserName);
                    
                    // Get user profile
                    var userProfile = await _authenticationService.GetUserProfileAsync(authResult.AccessToken);
                    
                    if (userProfile != null)
                    {
                        StatusMessage = $"Welcome, {userProfile.DisplayName}!";
                        
                        // Store user session information
                        Application.Current.Properties["CurrentUser"] = userProfile;
                        Application.Current.Properties["AccessToken"] = authResult.AccessToken;
                        
                        // Navigate to main application
                        await Task.Delay(1000); // Brief delay to show welcome message
                        ShowMainWindow();
                    }
                    else
                    {
                        throw new Exception("Failed to retrieve user profile");
                    }
                }
                else
                {
                    IsError = true;
                    StatusMessage = $"Authentication failed: {authResult.ErrorMessage}";
                    _logger.LogWarning("Authentication failed: {Error}", authResult.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                IsError = true;
                StatusMessage = $"An error occurred during authentication: {ex.Message}";
                _logger.LogError(ex, "Authentication error");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Command to handle offline/demo mode
        /// </summary>
        [RelayCommand]
        private async Task SignInOfflineAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Starting in offline mode...";

                _logger.LogInformation("Starting application in offline mode");

                // Create a demo user profile
                var demoUser = new UserProfile
                {
                    Id = "demo-user-id",
                    DisplayName = "Demo User",
                    Email = "demo@tradingaiassist.com",
                    GivenName = "Demo",
                    Surname = "User",
                    JobTitle = "Administrator",
                    Department = "IT"
                };

                // Store demo session
                Application.Current.Properties["CurrentUser"] = demoUser;
                Application.Current.Properties["IsOfflineMode"] = true;

                await Task.Delay(1000); // Brief delay
                ShowMainWindow();
            }
            catch (Exception ex)
            {
                IsError = true;
                StatusMessage = $"Error starting offline mode: {ex.Message}";
                _logger.LogError(ex, "Offline mode error");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Command to handle forgot password
        /// </summary>
        [RelayCommand]
        private void ForgotPassword()
        {
            try
            {
                StatusMessage = "Please contact your system administrator to reset your password.";
                _logger.LogInformation("User requested password reset");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling forgot password");
            }
        }

        /// <summary>
        /// Command to handle help
        /// </summary>
        [RelayCommand]
        private void ShowHelp()
        {
            try
            {
                MessageBox.Show(
                    "Trading AI Assist Admin Application\n\n" +
                    "To sign in:\n" +
                    "1. Click 'Sign In' to authenticate with Azure AD\n" +
                    "2. Use your organizational credentials\n" +
                    "3. For offline mode, click 'Offline Mode'\n\n" +
                    "For support, contact your system administrator.",
                    "Help",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error showing help");
            }
        }

        /// <summary>
        /// Shows the main window and closes the login window
        /// </summary>
        private void ShowMainWindow()
        {
            try
            {
                // Create and show main window
                var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                var mainWindowViewModel = _serviceProvider.GetRequiredService<MainWindowViewModel>();
                mainWindow.DataContext = mainWindowViewModel;
                mainWindow.Show();

                // Close the login window
                var loginWindow = Application.Current.Windows.OfType<LoginView>().FirstOrDefault();
                loginWindow?.Close();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error showing main window");
                MessageBox.Show($"Error starting application: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
} 
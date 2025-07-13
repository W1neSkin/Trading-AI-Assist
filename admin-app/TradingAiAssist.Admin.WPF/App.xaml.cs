using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Windows;
using TradingAiAssist.Admin.Core.Interfaces;
using TradingAiAssist.Admin.Services;
using TradingAiAssist.Admin.AzureAd.Services;
using TradingAiAssist.Admin.Data.Services;
using TradingAiAssist.Admin.Data.Configuration;
using TradingAiAssist.Admin.WPF.ViewModels;
using TradingAiAssist.Admin.WPF.Views;
using TradingAiAssist.Admin.WPF.Services;

namespace TradingAiAssist.Admin.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IHost? _host;
        private INavigationService? _navigationService;
        private MainWindowViewModel? _mainWindowViewModel;

        protected override async void OnStartup(StartupEventArgs e)
        {
            try
            {
                // Configure Serilog
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .WriteTo.Console()
                    .WriteTo.File("logs/trading-ai-assist-admin-.log", 
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 30)
                    .CreateLogger();

                Log.Information("Starting Trading AI Assist Admin Application");

                // Build host
                _host = Host.CreateDefaultBuilder()
                    .ConfigureServices((context, services) =>
                    {
                        ConfigureServices(services, context.Configuration);
                    })
                    .UseSerilog()
                    .Build();

                // Start the host
                await _host.StartAsync();

                // Get services from DI container
                _navigationService = _host.Services.GetRequiredService<INavigationService>();
                _mainWindowViewModel = _host.Services.GetRequiredService<MainWindowViewModel>();

                // Show login window first
                var loginViewModel = _host.Services.GetRequiredService<LoginViewModel>();
                var loginWindow = _host.Services.GetRequiredService<LoginView>();
                loginWindow.DataContext = loginViewModel;
                loginWindow.Show();

                Log.Information("Application started successfully");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application failed to start");
                MessageBox.Show($"Application failed to start: {ex.Message}", 
                    "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(1);
            }
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            try
            {
                Log.Information("Shutting down application");

                if (_host != null)
                {
                    await _host.StopAsync();
                    _host.Dispose();
                }

                Log.Information("Application shutdown complete");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during application shutdown");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Configuration
            services.AddSingleton(configuration);

            // API Options
            var apiOptions = new ApiOptions
            {
                BaseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:8000/",
                TimeoutSeconds = int.TryParse(configuration["ApiSettings:TimeoutSeconds"], out var timeout) ? timeout : 30,
                MaxRetries = int.TryParse(configuration["ApiSettings:MaxRetries"], out var retries) ? retries : 3
            };
            services.AddSingleton(apiOptions);

            // HTTP Client with retry policies
            services.AddHttpClientsWithRetry(apiOptions);

            // Azure AD Services
            services.AddSingleton<IAzureAdService, AzureAdService>();
            services.AddSingleton<IAuthenticationService, AuthenticationService>();

            // Data Services
            services.AddSingleton<IUserDataService, UserDataService>();
            services.AddSingleton<IAiAnalyticsDataService, AiAnalyticsDataService>();
            services.AddSingleton<ISystemHealthDataService, SystemHealthDataService>();
            services.AddSingleton<INotificationDataService, NotificationDataService>();

            // Navigation Service
            services.AddSingleton<INavigationService, NavigationService>();

            // Business Services
            services.AddSingleton<IUserManagementService, UserManagementService>();
            services.AddSingleton<IAiAnalyticsService, AiAnalyticsService>();
            services.AddSingleton<ISystemHealthService, SystemHealthService>();
            services.AddSingleton<INotificationService, NotificationService>();

            // ViewModels
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<UserManagementViewModel>();
            services.AddTransient<AiAnalyticsViewModel>();
            services.AddTransient<SystemHealthViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<UserEditDialogViewModel>();
            services.AddTransient<ConfirmationDialogViewModel>();

            // Views
            services.AddTransient<MainWindow>();
            services.AddTransient<LoginView>();
            services.AddTransient<DashboardView>();
            services.AddTransient<UserManagementView>();
            services.AddTransient<AiAnalyticsView>();
            services.AddTransient<SystemHealthView>();
            services.AddTransient<SettingsView>();
            services.AddTransient<UserEditDialog>();
            services.AddTransient<ConfirmationDialog>();

            // Background Services
            services.AddHostedService<SystemHealthMonitorService>();
            services.AddHostedService<AiUsageMonitorService>();

            // WebSocket Service
            services.AddSingleton<WebSocketService>();
        }
    }
} 
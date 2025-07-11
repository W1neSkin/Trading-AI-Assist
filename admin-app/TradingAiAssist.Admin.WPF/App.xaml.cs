using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Windows;
using TradingAiAssist.Admin.Core.Interfaces;
using TradingAiAssist.Admin.Services;
using TradingAiAssist.Admin.AzureAd.Services;
using TradingAiAssist.Admin.Data.Services;
using TradingAiAssist.Admin.WPF.ViewModels;
using TradingAiAssist.Admin.WPF.Views;

namespace TradingAiAssist.Admin.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IHost? _host;

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

                // Show main window
                var mainWindow = _host.Services.GetRequiredService<MainWindow>();
                mainWindow.Show();

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

            // HTTP Client
            services.AddHttpClient("TradingAiAssistApi", client =>
            {
                client.BaseAddress = new Uri(configuration["ApiSettings:BaseUrl"] ?? "https://localhost:8000/");
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            // Azure AD Services
            services.AddSingleton<IAzureAdService, AzureAdService>();
            services.AddSingleton<IAuthenticationService, AuthenticationService>();

            // Data Services
            services.AddSingleton<IUserDataService, UserDataService>();
            services.AddSingleton<IAiAnalyticsDataService, AiAnalyticsDataService>();
            services.AddSingleton<ISystemHealthDataService, SystemHealthDataService>();

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

            // Views
            services.AddTransient<MainWindow>();
            services.AddTransient<LoginView>();
            services.AddTransient<DashboardView>();
            services.AddTransient<UserManagementView>();
            services.AddTransient<AiAnalyticsView>();
            services.AddTransient<SystemHealthView>();
            services.AddTransient<SettingsView>();

            // Background Services
            services.AddHostedService<SystemHealthMonitorService>();
            services.AddHostedService<AiUsageMonitorService>();
        }
    }
} 
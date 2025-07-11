using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TradingAiAssist.Admin.WPF.Services;

namespace TradingAiAssist.Admin.WPF.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private bool _isMenuExpanded = true;

        public MainWindowViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            
            // Subscribe to navigation changes
            _navigationService.NavigationChanged += OnNavigationChanged;

            // Initialize commands
            NavigateToDashboardCommand = new RelayCommand(() => _navigationService.NavigateTo<DashboardViewModel>());
            NavigateToUsersCommand = new RelayCommand(() => _navigationService.NavigateTo<UserManagementViewModel>());
            NavigateToAnalyticsCommand = new RelayCommand(() => _navigationService.NavigateTo<AiAnalyticsViewModel>());
            NavigateToHealthCommand = new RelayCommand(() => _navigationService.NavigateTo<SystemHealthViewModel>());
            NavigateToSettingsCommand = new RelayCommand(() => _navigationService.NavigateTo<SettingsViewModel>());
            ToggleMenuCommand = new RelayCommand(() => IsMenuExpanded = !IsMenuExpanded);
            GoBackCommand = new RelayCommand(() => _navigationService.GoBack(), () => _navigationService.CanGoBack);

            // Set initial view
            _navigationService.NavigateTo<DashboardViewModel>();
        }

        public BaseViewModel? CurrentViewModel => _navigationService.CurrentViewModel;
        public bool CanGoBack => _navigationService.CanGoBack;

        public bool IsMenuExpanded
        {
            get => _isMenuExpanded;
            set => SetProperty(ref _isMenuExpanded, value);
        }

        public ICommand NavigateToDashboardCommand { get; }
        public ICommand NavigateToUsersCommand { get; }
        public ICommand NavigateToAnalyticsCommand { get; }
        public ICommand NavigateToHealthCommand { get; }
        public ICommand NavigateToSettingsCommand { get; }
        public ICommand ToggleMenuCommand { get; }
        public ICommand GoBackCommand { get; }

        private void OnNavigationChanged(object? sender, NavigationEventArgs e)
        {
            OnPropertyChanged(nameof(CurrentViewModel));
            OnPropertyChanged(nameof(CanGoBack));
        }
    }
} 
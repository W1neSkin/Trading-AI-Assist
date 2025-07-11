using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TradingAiAssist.Admin.Core.Models;
using TradingAiAssist.Admin.Core.Services;

namespace TradingAiAssist.Admin.WPF.ViewModels
{
    public class UserManagementViewModel : INotifyPropertyChanged
    {
        private readonly IUserManagementService _userService;
        private string _searchTerm = "";
        private User? _selectedUser;
        private Department? _selectedDepartment;
        private Role? _selectedRole;
        private int _currentPage = 1;
        private int _selectedPageSize = 20;
        private bool _isLoading;

        public UserManagementViewModel(IUserManagementService userService)
        {
            _userService = userService;
            
            Users = new ObservableCollection<User>();
            Departments = new ObservableCollection<Department>();
            Roles = new ObservableCollection<Role>();
            PageSizes = new ObservableCollection<int> { 10, 20, 50, 100 };

            // Initialize commands
            AddUserCommand = new RelayCommand(() => AddUser());
            EditUserCommand = new RelayCommand(() => EditUser(), () => SelectedUser != null);
            DeleteUserCommand = new RelayCommand(async () => await DeleteUserAsync(), () => SelectedUser != null);
            SearchCommand = new RelayCommand(async () => await SearchUsersAsync());
            BulkImportCommand = new RelayCommand(() => BulkImport());
            ExportCommand = new RelayCommand(async () => await ExportUsersAsync());
            PreviousPageCommand = new RelayCommand(() => PreviousPage(), () => CurrentPage > 1);
            NextPageCommand = new RelayCommand(() => NextPage(), () => CurrentPage < TotalPages);

            // Initialize data
            _ = LoadInitialDataAsync();
        }

        public string SearchTerm
        {
            get => _searchTerm;
            set => SetProperty(ref _searchTerm, value);
        }

        public User? SelectedUser
        {
            get => _selectedUser;
            set
            {
                SetProperty(ref _selectedUser, value);
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public Department? SelectedDepartment
        {
            get => _selectedDepartment;
            set => SetProperty(ref _selectedDepartment, value);
        }

        public Role? SelectedRole
        {
            get => _selectedRole;
            set => SetProperty(ref _selectedRole, value);
        }

        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                SetProperty(ref _currentPage, value);
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public int SelectedPageSize
        {
            get => _selectedPageSize;
            set
            {
                SetProperty(ref _selectedPageSize, value);
                _ = LoadUsersAsync();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public int TotalUsers { get; private set; }
        public int TotalPages { get; private set; }

        public ObservableCollection<User> Users { get; }
        public ObservableCollection<Department> Departments { get; }
        public ObservableCollection<Role> Roles { get; }
        public ObservableCollection<int> PageSizes { get; }

        public ICommand AddUserCommand { get; }
        public ICommand EditUserCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand BulkImportCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand NextPageCommand { get; }

        private async Task LoadInitialDataAsync()
        {
            IsLoading = true;
            try
            {
                await Task.WhenAll(
                    LoadUsersAsync(),
                    LoadDepartmentsAsync(),
                    LoadRolesAsync()
                );
            }
            catch (Exception ex)
            {
                // Handle error - could show a message to user
                System.Diagnostics.Debug.WriteLine($"Error loading initial data: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadUsersAsync()
        {
            try
            {
                var result = await _userService.GetUsersAsync(new UserSearchCriteria
                {
                    SearchTerm = SearchTerm,
                    DepartmentId = SelectedDepartment?.Id,
                    RoleId = SelectedRole?.Id,
                    Page = CurrentPage,
                    PageSize = SelectedPageSize
                });

                Users.Clear();
                foreach (var user in result.Users)
                {
                    Users.Add(user);
                }

                TotalUsers = result.TotalCount;
                TotalPages = (int)Math.Ceiling((double)TotalUsers / SelectedPageSize);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading users: {ex.Message}");
            }
        }

        private async Task LoadDepartmentsAsync()
        {
            try
            {
                var departments = await _userService.GetDepartmentsAsync();
                Departments.Clear();
                foreach (var dept in departments)
                {
                    Departments.Add(dept);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading departments: {ex.Message}");
            }
        }

        private async Task LoadRolesAsync()
        {
            try
            {
                var roles = await _userService.GetRolesAsync();
                Roles.Clear();
                foreach (var role in roles)
                {
                    Roles.Add(role);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading roles: {ex.Message}");
            }
        }

        private async Task SearchUsersAsync()
        {
            CurrentPage = 1;
            await LoadUsersAsync();
        }

        private void AddUser()
        {
            // TODO: Open add user dialog
            System.Diagnostics.Debug.WriteLine("Add user clicked");
        }

        private void EditUser()
        {
            if (SelectedUser != null)
            {
                // TODO: Open edit user dialog
                System.Diagnostics.Debug.WriteLine($"Edit user: {SelectedUser.DisplayName}");
            }
        }

        private async Task DeleteUserAsync()
        {
            if (SelectedUser != null)
            {
                // TODO: Show confirmation dialog
                try
                {
                    await _userService.DeleteUserAsync(SelectedUser.Id);
                    await LoadUsersAsync();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error deleting user: {ex.Message}");
                }
            }
        }

        private void BulkImport()
        {
            // TODO: Open file dialog and import users
            System.Diagnostics.Debug.WriteLine("Bulk import clicked");
        }

        private async Task ExportUsersAsync()
        {
            try
            {
                // TODO: Implement export functionality
                System.Diagnostics.Debug.WriteLine("Export users clicked");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error exporting users: {ex.Message}");
            }
        }

        private void PreviousPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                _ = LoadUsersAsync();
            }
        }

        private void NextPage()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                _ = LoadUsersAsync();
            }
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
} 
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using TradingAiAssist.Admin.Core.Interfaces;
using TradingAiAssist.Admin.Core.Models;

namespace TradingAiAssist.Admin.WPF.ViewModels
{
    /// <summary>
    /// ViewModel for the user edit dialog
    /// </summary>
    public partial class UserEditDialogViewModel : BaseViewModel
    {
        private readonly IUserManagementService _userManagementService;
        private readonly ILogger<UserEditDialogViewModel> _logger;

        [ObservableProperty]
        private string _title = "Add New User";

        [ObservableProperty]
        private string _displayName = "";

        [ObservableProperty]
        private string _email = "";

        [ObservableProperty]
        private string _firstName = "";

        [ObservableProperty]
        private string _lastName = "";

        [ObservableProperty]
        private string _jobTitle = "";

        [ObservableProperty]
        private string _department = "";

        [ObservableProperty]
        private string _phoneNumber = "";

        [ObservableProperty]
        private string _officeLocation = "";

        [ObservableProperty]
        private UserRole _selectedRole = UserRole.Viewer;

        [ObservableProperty]
        private bool _isActive = true;

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private string _statusMessage = "";

        [ObservableProperty]
        private bool _isError = false;

        [ObservableProperty]
        private bool _isEditMode = false;

        [ObservableProperty]
        private string _userId = "";

        public ObservableCollection<UserRole> AvailableRoles { get; } = new()
        {
            UserRole.SuperAdmin,
            UserRole.Admin, 
            UserRole.Support,
            UserRole.Compliance,
            UserRole.Financial,
            UserRole.AIAdmin,
            UserRole.Viewer
        };

        public ObservableCollection<string> AvailableDepartments { get; } = new()
        {
            "IT",
            "Finance",
            "Operations",
            "Compliance",
            "Support",
            "Sales",
            "Marketing",
            "HR"
        };

        public UserEditDialogViewModel(
            IUserManagementService userManagementService,
            ILogger<UserEditDialogViewModel> logger)
        {
            _userManagementService = userManagementService;
            _logger = logger;
        }

        /// <summary>
        /// Initialize the dialog for editing an existing user
        /// </summary>
        public void InitializeForEdit(UserProfile user)
        {
            IsEditMode = true;
            UserId = user.Id.ToString();
            Title = "Edit User";
            
            DisplayName = user.DisplayName ?? "";
            Email = user.Email ?? "";
            FirstName = user.FirstName ?? "";
            LastName = user.LastName ?? "";
            JobTitle = user.JobTitle ?? "";
            Department = user.Department ?? "";
            PhoneNumber = user.PhoneNumber ?? "";
            OfficeLocation = user.OfficeLocation ?? "";
            SelectedRole = user.Role;
            IsActive = user.IsActive;
        }

        /// <summary>
        /// Command to save the user
        /// </summary>
        [RelayCommand]
        private async Task SaveAsync()
        {
            try
            {
                if (!ValidateInput())
                {
                    return;
                }

                IsLoading = true;
                IsError = false;
                StatusMessage = IsEditMode ? "Updating user..." : "Creating user...";

                var userProfile = new UserProfile
                {
                    Id = IsEditMode ? Guid.Parse(UserId) : Guid.NewGuid(),
                    DisplayName = DisplayName.Trim(),
                    Email = Email.Trim().ToLower(),
                    FirstName = FirstName.Trim(),
                    LastName = LastName.Trim(),
                    JobTitle = JobTitle.Trim(),
                    Department = Department.Trim(),
                    PhoneNumber = PhoneNumber.Trim(),
                    OfficeLocation = OfficeLocation.Trim(),
                    Role = SelectedRole,
                    IsActive = IsActive,
                    CreatedAt = IsEditMode ? DateTime.Now : DateTime.Now // In edit mode, keep original creation date
                };

                bool success;
                if (IsEditMode)
                {
                    success = await _userManagementService.UpdateUserAsync(userProfile);
                    _logger.LogInformation("User updated: {Email}", userProfile.Email);
                }
                else
                {
                    success = await _userManagementService.CreateUserAsync(userProfile);
                    _logger.LogInformation("User created: {Email}", userProfile.Email);
                }

                if (success)
                {
                    StatusMessage = IsEditMode ? "User updated successfully!" : "User created successfully!";
                    await Task.Delay(1000); // Brief delay to show success message
                    
                    // Close the dialog with success result
                    CloseDialog(true);
                }
                else
                {
                    IsError = true;
                    StatusMessage = IsEditMode ? "Failed to update user. Please try again." : "Failed to create user. Please try again.";
                }
            }
            catch (Exception ex)
            {
                IsError = true;
                StatusMessage = $"An error occurred: {ex.Message}";
                _logger.LogError(ex, "Error saving user");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Command to cancel the operation
        /// </summary>
        [RelayCommand]
        private void Cancel()
        {
            CloseDialog(false);
        }

        /// <summary>
        /// Validates the input fields
        /// </summary>
        private bool ValidateInput()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(DisplayName))
                errors.Add("Display name is required");

            if (string.IsNullOrWhiteSpace(Email))
                errors.Add("Email is required");
            else if (!IsValidEmail(Email))
                errors.Add("Please enter a valid email address");

            if (string.IsNullOrWhiteSpace(Department))
                errors.Add("Department is required");

            if (errors.Any())
            {
                IsError = true;
                StatusMessage = string.Join("\n", errors);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates email format
        /// </summary>
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Closes the dialog with the specified result
        /// </summary>
        private void CloseDialog(bool result)
        {
            // Find the dialog window and set the result
            var dialog = Application.Current.Windows.OfType<Window>()
                .FirstOrDefault(w => w.DataContext == this);
            
            if (dialog != null)
            {
                dialog.DialogResult = result;
                dialog.Close();
            }
        }
    }
} 
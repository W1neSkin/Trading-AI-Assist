using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace TradingAiAssist.Admin.WPF.ViewModels
{
    /// <summary>
    /// ViewModel for confirmation dialogs
    /// </summary>
    public partial class ConfirmationDialogViewModel : BaseViewModel
    {
        private readonly ILogger<ConfirmationDialogViewModel> _logger;

        [ObservableProperty]
        private string _title = "Confirm Action";

        [ObservableProperty]
        private string _message = "Are you sure you want to perform this action?";

        [ObservableProperty]
        private string _confirmButtonText = "Confirm";

        [ObservableProperty]
        private string _cancelButtonText = "Cancel";

        [ObservableProperty]
        private bool _isDestructive = false;

        [ObservableProperty]
        private bool _isLoading = false;

        public ConfirmationDialogViewModel(ILogger<ConfirmationDialogViewModel> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Initialize the dialog with custom settings
        /// </summary>
        public void Initialize(string title, string message, string confirmButtonText = "Confirm", 
                             string cancelButtonText = "Cancel", bool isDestructive = false)
        {
            Title = title;
            Message = message;
            ConfirmButtonText = confirmButtonText;
            CancelButtonText = cancelButtonText;
            IsDestructive = isDestructive;
        }

        /// <summary>
        /// Command to confirm the action
        /// </summary>
        [RelayCommand]
        private void Confirm()
        {
            try
            {
                IsLoading = true;
                _logger.LogInformation("User confirmed action: {Title}", Title);
                
                // Close the dialog with true result
                CloseDialog(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in confirmation dialog");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Command to cancel the action
        /// </summary>
        [RelayCommand]
        private void Cancel()
        {
            try
            {
                _logger.LogInformation("User cancelled action: {Title}", Title);
                
                // Close the dialog with false result
                CloseDialog(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in confirmation dialog");
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
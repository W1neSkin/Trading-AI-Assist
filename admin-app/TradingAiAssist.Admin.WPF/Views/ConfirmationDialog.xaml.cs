using System.Windows;
using TradingAiAssist.Admin.WPF.ViewModels;

namespace TradingAiAssist.Admin.WPF.Views
{
    /// <summary>
    /// Interaction logic for ConfirmationDialog.xaml
    /// </summary>
    public partial class ConfirmationDialog : Window
    {
        public ConfirmationDialog(ConfirmationDialogViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
} 
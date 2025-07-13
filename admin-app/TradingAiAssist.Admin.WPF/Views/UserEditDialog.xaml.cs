using System.Windows;
using TradingAiAssist.Admin.WPF.ViewModels;

namespace TradingAiAssist.Admin.WPF.Views
{
    /// <summary>
    /// Interaction logic for UserEditDialog.xaml
    /// </summary>
    public partial class UserEditDialog : Window
    {
        public UserEditDialog(UserEditDialogViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
} 
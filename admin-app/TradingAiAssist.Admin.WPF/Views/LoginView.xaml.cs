using System.Windows;
using TradingAiAssist.Admin.WPF.ViewModels;

namespace TradingAiAssist.Admin.WPF.Views
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : Window
    {
        public LoginView(LoginViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
} 
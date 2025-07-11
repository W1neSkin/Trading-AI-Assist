using System.Windows.Controls;

namespace TradingAiAssist.Admin.WPF.Views
{
    /// <summary>
    /// Interaction logic for UserManagementView.xaml
    /// </summary>
    public partial class UserManagementView : UserControl
    {
        public UserManagementView()
        {
            InitializeComponent();
        }

        private void DataGridRow_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DataContext is ViewModels.UserManagementViewModel viewModel)
            {
                viewModel.EditUserCommand.Execute(null);
            }
        }
    }
} 
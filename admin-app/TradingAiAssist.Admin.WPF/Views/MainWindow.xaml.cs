using System.Windows;
using TradingAiAssist.Admin.WPF.ViewModels;

namespace TradingAiAssist.Admin.WPF.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
} 
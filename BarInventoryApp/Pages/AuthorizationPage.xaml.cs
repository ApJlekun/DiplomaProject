using BarInventoryApp.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace BarInventoryApp.Pages
{
    public partial class AuthorizationPage : Page
    {
        private readonly AuthorizationViewModel _viewModel;

        public AuthorizationPage(AuthorizationViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = viewModel;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                _viewModel.Password = passwordBox.Password;
            }
        }

        private void OnExitClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}

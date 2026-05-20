using BarInventoryApp.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BarInventoryApp.Pages
{
    public partial class IngredientsPage : Page
    {
        private readonly IngredientsViewModel _viewModel;

        public IngredientsPage(IngredientsViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            var role = Utils.Session.CurrentUser?.Role.Name;
            if (role == "Admin") _viewModel.NavigateTo<AdminDashboardPage>();
            else _viewModel.NavigateTo<ManagerDashboardPage>();
        }

        private void OnLogoutClick(object sender, RoutedEventArgs e)
        {
            Utils.Session.CurrentUser = null;
            _viewModel.NavigateTo<AuthorizationPage>();
        }

        private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is IngredientsViewModel viewModel)
            {
                viewModel.OpenCategoryCommand?.Execute(null);
            }
        }
    }
}

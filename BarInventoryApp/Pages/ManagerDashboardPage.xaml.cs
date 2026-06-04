using BarInventoryApp.Utils;
using BarInventoryApp.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace BarInventoryApp.Pages
{
    public partial class ManagerDashboardPage : Page
    {
        private readonly MainViewModel _mainViewModel;

        public ManagerDashboardPage(MainViewModel mainViewModel)
        {
            InitializeComponent();
            _mainViewModel = mainViewModel;
        }

        private void OnIngredientsClick(object sender, RoutedEventArgs e)
        {
            _mainViewModel.NavigateTo<IngredientsPage>();
        }

        private void OnOrdersClick(object sender, RoutedEventArgs e)
        {
            _mainViewModel.NavigateTo<OrdersPage>();
        }

        private void OnCocktailsClick(object sender, RoutedEventArgs e)
        {
            _mainViewModel.NavigateTo<CocktailsPage>();
        }

        private void OnLogoutClick(object sender, RoutedEventArgs e)
        {
            Session.CurrentUser = null;
            _mainViewModel.NavigateTo<AuthorizationPage>();
        }
    }
}

using BarInventoryApp.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BarInventoryApp.Pages
{
    public partial class OrdersPage : Page
    {
        public OrdersPage(OrdersViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is OrdersViewModel viewModel)
            {
                viewModel.BackCommand.Execute(null);
            }
        }

        private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is OrdersViewModel viewModel)
            {
                viewModel.EditCommand?.Execute(null);
            }
        }
    }
}

using BarInventoryApp.ViewModels;
using System.Windows;

namespace BarInventoryApp.Pages
{
    public partial class ReceiptDetailsDialog : Window
    {
        public ReceiptDetailsDialog(ReceiptDetailsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

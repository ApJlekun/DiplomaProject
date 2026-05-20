using System.Windows;

namespace BarInventoryApp.Pages
{
    public partial class InvoiceEditDialog : Window
    {
        public InvoiceEditDialog(object viewModel)
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

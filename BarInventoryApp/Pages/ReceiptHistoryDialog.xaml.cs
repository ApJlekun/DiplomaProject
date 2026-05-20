using BarInventoryApp.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace BarInventoryApp.Pages
{
    public partial class ReceiptHistoryDialog : Window
    {
        public ReceiptHistoryDialog(ReceiptHistoryViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.CloseRequested += () => Close();
        }

        private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is ReceiptHistoryViewModel viewModel)
            {
                viewModel.ShowDetailsCommand.Execute(null);
            }
        }
    }
}

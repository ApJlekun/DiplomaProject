using BarInventoryApp.ViewModels;
using System.Windows;

namespace BarInventoryApp.Pages
{
    public partial class CreateReceiptDialog : Window
    {
        public CreateReceiptDialog(CreateReceiptViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.CloseRequested += () => DialogResult = true;
        }
    }
}

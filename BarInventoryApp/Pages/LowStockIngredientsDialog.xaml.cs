using BarInventoryApp.ViewModels;
using System.Windows;

namespace BarInventoryApp.Pages
{
    public partial class LowStockIngredientsDialog : Window
    {
        public LowStockIngredientsDialog(LowStockIngredientsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.CloseRequested += () => Close();
        }
    }
}

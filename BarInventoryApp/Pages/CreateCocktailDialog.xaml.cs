using BarInventoryApp.ViewModels;
using System.Windows;

namespace BarInventoryApp.Pages
{
    public partial class CreateCocktailDialog : Window
    {
        public CreateCocktailDialog(CreateCocktailViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.CloseRequested += () => { DialogResult = true; Close(); };
        }
    }
}

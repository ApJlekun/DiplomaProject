using System.Windows;

namespace BarInventoryApp.Pages
{
    public partial class CategoryIngredientsDialog : Window
    {
        public CategoryIngredientsDialog(object viewModel)
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

using BarInventoryApp.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BarInventoryApp.Pages
{
    public partial class CocktailsPage : Page
    {
        public CocktailsPage(CocktailsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is CocktailsViewModel viewModel)
            {
                viewModel.BackCommand.Execute(null);
            }
        }

        private void OnCocktailLeftClick(object sender, MouseButtonEventArgs e)
        {
            var grid = sender as DataGrid;
            if (grid == null) return;

            var row = FindVisualParent<DataGridRow>((DependencyObject)e.OriginalSource);
            if (row != null)
            {
                grid.SelectedItem = row.Item;
                if (grid.ContextMenu != null)
                {
                    grid.ContextMenu.PlacementTarget = grid;
                    grid.ContextMenu.DataContext = this.DataContext; 
                    grid.ContextMenu.IsOpen = true;
                    e.Handled = true;
                }
            }
        }

        private static T? FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            if (parentObject is T parent) return parent;
            return FindVisualParent<T>(parentObject);
        }
    }
}

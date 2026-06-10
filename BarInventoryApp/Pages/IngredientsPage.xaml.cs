using BarInventoryApp.Constants;
using BarInventoryApp.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BarInventoryApp.Pages
{
    public partial class IngredientsPage : Page
    {
        private readonly IngredientsViewModel _viewModel;

        public IngredientsPage(IngredientsViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;

            if (Utils.Session.CurrentUser?.Role.Name == ApplicationConstants.Roles.Barmen)
                BackButton.Visibility = Visibility.Collapsed;
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            var role = Utils.Session.CurrentUser?.Role.Name;
            if (ApplicationConstants.Roles.IsAdmin(role))
                _viewModel.NavigateTo<AdminDashboardPage>();
            else if (role == ApplicationConstants.Roles.Manager)
                _viewModel.NavigateTo<ManagerDashboardPage>();
        }

        private void OnLogoutClick(object sender, RoutedEventArgs e)
        {
            Utils.Session.CurrentUser = null;
            _viewModel.NavigateTo<AuthorizationPage>();
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.IsEditable)
            {
                var textBox = comboBox.Template.FindName("PART_EditableTextBox", comboBox) as TextBox;
                if (textBox != null)
                {
                    // Если текст выделен полностью (автовыделение WPF), сбрасываем его
                    if (textBox.SelectionLength > 0 && textBox.SelectionLength == textBox.Text.Length)
                    {
                        textBox.SelectionLength = 0;
                        textBox.CaretIndex = textBox.Text.Length;
                    }
                }
            }
        }

        private void OnDataGridPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var grid = sender as DataGrid;
            if (grid == null) return;

            var cell = FindVisualParent<DataGridCell>((DependencyObject)e.OriginalSource);
            if (cell != null && grid.Columns.Contains(cell.Column))
            {
                var row = FindVisualParent<DataGridRow>(cell);
                if (row != null && row.IsSelected)
                {
                    row.IsSelected = false;
                    grid.SelectedItem = null;
                    e.Handled = true;
                }
            }
        }

        private void OnIngredientLeftClick(object sender, MouseButtonEventArgs e)
        {
            var grid = sender as DataGrid;
            if (grid == null) return;

            if (Utils.Session.CurrentUser?.Role.Name == ApplicationConstants.Roles.Barmen)
                return;

            var row = FindVisualParent<DataGridRow>((DependencyObject)e.OriginalSource);
            if (row != null)
            {
                grid.SelectedItem = row.Item;
                if (grid.ContextMenu != null)
                {
                    grid.ContextMenu.PlacementTarget = grid;
                    // ПЕРЕДАЕМ DATACOTEXT СТРАНИЦЫ В МЕНЮ, ЧТОБЫ КОМАНДЫ ЗАРАБОТАЛИ
                    grid.ContextMenu.DataContext = this.DataContext; 
                    grid.ContextMenu.IsOpen = true;
                    e.Handled = true;
                }
            }
        }

        private void OnIngredientRightClick(object sender, MouseButtonEventArgs e)
        {
            var grid = sender as DataGrid;
            if (grid == null) return;

            var row = FindVisualParent<DataGridRow>((DependencyObject)e.OriginalSource);
            if (row != null)
            {
                grid.SelectedItem = row.Item;
                // Мы НЕ открываем контекстное меню здесь, 
                // так как оно привязано к DataGrid.ContextMenu и откроется само.
                // Чтобы предотвратить авто-открытие (если нужно только выделение):
                if (grid.ContextMenu != null)
                {
                    grid.ContextMenu.IsOpen = false; 
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

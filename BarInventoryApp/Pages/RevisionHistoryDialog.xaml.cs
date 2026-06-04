using BarInventoryApp.ViewModels;
using System.Windows;

namespace BarInventoryApp.Pages
{
    public partial class RevisionHistoryDialog : Window
    {
        private readonly RevisionHistoryViewModel _viewModel;

        public RevisionHistoryDialog(RevisionHistoryViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        private void OnMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _viewModel.OpenCommand.Execute(null);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

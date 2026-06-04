using BarInventoryApp.ViewModels;
using System.Windows;

namespace BarInventoryApp.Pages
{
    public partial class RevisionDialog : Window
    {
        public RevisionDialog(RevisionViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

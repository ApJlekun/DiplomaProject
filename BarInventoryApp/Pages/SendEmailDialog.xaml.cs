using System.Windows;

namespace BarInventoryApp.Pages
{
    public partial class SendEmailDialog : Window
    {
        public string RecipientEmail { get; private set; } = string.Empty;

        public SendEmailDialog()
        {
            InitializeComponent();
            EmailTextBox.Focus();
        }

        private void OnSendClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EmailTextBox.Text) || !EmailTextBox.Text.Contains("@"))
            {
                MessageBox.Show("Введите корректный Email адрес.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            RecipientEmail = EmailTextBox.Text.Trim();
            DialogResult = true;
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}

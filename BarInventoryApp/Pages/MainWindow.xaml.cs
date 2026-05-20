using BarInventoryApp.ViewModels;
using System;
using System.Windows;

namespace BarInventoryApp.Pages
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private bool _isDarkTheme = false;

        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;

            // Передаём Frame после инициализации
            _viewModel.SetFrame(MainFrame);
        }

        private void OnThemeToggleClick(object sender, RoutedEventArgs e)
        {
            _isDarkTheme = !_isDarkTheme;

            var newThemeUri = _isDarkTheme 
                ? new Uri("pack://application:,,,/BarInventoryApp;component/Themes/DarkTheme.xaml")
                : new Uri("pack://application:,,,/BarInventoryApp;component/Themes/LightTheme.xaml");

            var newDict = new ResourceDictionary { Source = newThemeUri };

            // Очищаем текущие словари и добавляем новый
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(newDict);

            // Меняем иконку кнопки
            ThemeToggleButton.Content = _isDarkTheme ? "☀️" : "🌙";
        }
    }
}

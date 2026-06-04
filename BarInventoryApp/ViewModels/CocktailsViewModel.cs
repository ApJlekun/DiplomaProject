using BarInventoryApp.Models;
using BarInventoryApp.Services;
using BarInventoryApp.Utils;
using BarInventoryApp.Pages;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System;
using BarInventoryApp.Constants;

namespace BarInventoryApp.ViewModels
{
    public class CocktailsViewModel : INotifyPropertyChanged
    {
        private readonly CocktailService _cocktailService;
        private readonly IngredientService _ingredientService;
        private readonly MainViewModel _mainViewModel;
        private Cocktail? _selectedCocktail;

        public ObservableCollection<Cocktail> Cocktails { get; } = new();

        public Cocktail? SelectedCocktail
        {
            get => _selectedCocktail;
            set { _selectedCocktail = value; OnPropertyChanged(); }
        }

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand BackCommand { get; }

        public CocktailsViewModel(CocktailService cocktailService, IngredientService ingredientService, MainViewModel mainViewModel)
        {
            _cocktailService = cocktailService;
            _ingredientService = ingredientService;
            _mainViewModel = mainViewModel;

            AddCommand = new RelayCommand(OnAdd);
            EditCommand = new RelayCommand(OnEdit);
            DeleteCommand = new RelayCommand(OnDelete);
            BackCommand = new RelayCommand(OnBack);

            LoadCocktails();
        }

        private async void LoadCocktails()
        {
            var list = await _cocktailService.GetAllCocktailsAsync();
            Cocktails.Clear();
            foreach (var c in list) Cocktails.Add(c);
        }

        private void OnAdd()
        {
            var viewModel = new CreateCocktailViewModel(_cocktailService, _ingredientService, null);
            var dialog = new CreateCocktailDialog(viewModel);
            if (dialog.ShowDialog() == true)
            {
                LoadCocktails();
            }
        }

        private void OnEdit()
        {
            if (SelectedCocktail == null) return;
            var viewModel = new CreateCocktailViewModel(_cocktailService, _ingredientService, SelectedCocktail);
            var dialog = new CreateCocktailDialog(viewModel);
            if (dialog.ShowDialog() == true)
            {
                LoadCocktails();
            }
        }

        private async void OnDelete()
        {
            if (SelectedCocktail == null) return;

            var res = MessageBox.Show($"Удалить коктейль '{SelectedCocktail.Name}'?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes)
            {
                await _cocktailService.DeleteCocktailAsync(SelectedCocktail.Id);
                LoadCocktails();
            }
        }

        private void OnBack()
        {
            var role = Session.CurrentUser?.Role.Name;
            if (role == ApplicationConstants.Roles.Admin) _mainViewModel.NavigateTo<AdminDashboardPage>();
            else if (role == ApplicationConstants.Roles.Manager) _mainViewModel.NavigateTo<ManagerDashboardPage>();
            else _mainViewModel.NavigateTo<AuthorizationPage>();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

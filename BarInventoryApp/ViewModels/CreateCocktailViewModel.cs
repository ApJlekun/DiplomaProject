using BarInventoryApp.Models;
using BarInventoryApp.Services;
using BarInventoryApp.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace BarInventoryApp.ViewModels
{
    public class CreateCocktailViewModel : INotifyPropertyChanged
    {
        private readonly CocktailService _cocktailService;
        private readonly IngredientService _ingredientService;
        private readonly Cocktail? _existingCocktail;
        private string _name = string.Empty;
        private string _searchText = string.Empty;
        private decimal _quantity;
        private Ingredient? _selectedIngredient;
        private bool _isUpdating;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText == value) return;
                _searchText = value;
                OnPropertyChanged();
                if (!_isUpdating)
                {
                    _selectedIngredient = null;
                    OnPropertyChanged(nameof(SelectedIngredient));
                    SearchIngredients();
                }
            }
        }

        public decimal Quantity
        {
            get => _quantity;
            set { _quantity = value; OnPropertyChanged(); }
        }

        public Ingredient? SelectedIngredient
        {
            get => _selectedIngredient;
            set
            {
                if (_isUpdating) return;
                if (value == null && _selectedIngredient != null && _searchText == _selectedIngredient.Name) return;
                _selectedIngredient = value;
                if (_selectedIngredient != null)
                {
                    _isUpdating = true;
                    SearchText = _selectedIngredient.Name;
                    FoundIngredients.Clear();
                    _isUpdating = false;
                }
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Ingredient> FoundIngredients { get; } = new();
        public ObservableCollection<CocktailIngredientViewModel> CocktailIngredients { get; } = new();

        public ICommand AddIngredientCommand { get; }
        public ICommand RemoveIngredientCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action? CloseRequested;

        public CreateCocktailViewModel(CocktailService cocktailService, IngredientService ingredientService, Cocktail? existingCocktail)
        {
            _cocktailService = cocktailService;
            _ingredientService = ingredientService;
            _existingCocktail = existingCocktail;

            AddIngredientCommand = new RelayCommand(OnAddIngredient);
            RemoveIngredientCommand = new RelayCommand<CocktailIngredientViewModel>(OnRemoveIngredient);
            SaveCommand = new RelayCommand(OnSave);
            CancelCommand = new RelayCommand(() => CloseRequested?.Invoke());

            if (_existingCocktail != null)
            {
                Name = _existingCocktail.Name;
                foreach (var ci in _existingCocktail.CocktailIngredients)
                {
                    CocktailIngredients.Add(new CocktailIngredientViewModel
                    {
                        IngredientId = ci.IngredientId,
                        IngredientName = ci.Ingredient.Name,
                        Quantity = ci.Quantity,
                        Unit = ci.Ingredient.Unit
                    });
                }
            }
        }

        private async void SearchIngredients()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FoundIngredients.Clear();
                return;
            }

            var results = await _ingredientService.SearchAsync(SearchText);
            if (!_isUpdating)
            {
                FoundIngredients.Clear();
                foreach (var item in results) FoundIngredients.Add(item);
            }
        }

        private void OnAddIngredient()
        {
            if (SelectedIngredient == null)
            {
                MessageBox.Show("Выберите ингредиент.");
                return;
            }
            if (Quantity <= 0)
            {
                MessageBox.Show("Введите количество.");
                return;
            }

            CocktailIngredients.Add(new CocktailIngredientViewModel
            {
                IngredientId = SelectedIngredient.Id,
                IngredientName = SelectedIngredient.Name,
                Quantity = Quantity,
                Unit = SelectedIngredient.Unit
            });

            _isUpdating = true;
            SearchText = string.Empty;
            Quantity = 0;
            _selectedIngredient = null;
            OnPropertyChanged(nameof(SelectedIngredient));
            FoundIngredients.Clear();
            _isUpdating = false;
        }

        private void OnRemoveIngredient(CocktailIngredientViewModel? item)
        {
            if (item != null) CocktailIngredients.Remove(item);
        }

        private async void OnSave()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                MessageBox.Show("Введите название коктейля.");
                return;
            }
            if (CocktailIngredients.Count == 0)
            {
                MessageBox.Show("Добавьте хотя бы один ингредиент.");
                return;
            }

            try
            {
                var ingredients = CocktailIngredients.Select(ci => new CocktailIngredient
                {
                    IngredientId = ci.IngredientId,
                    Quantity = ci.Quantity
                }).ToList();

                if (_existingCocktail == null)
                {
                    await _cocktailService.AddCocktailAsync(new Cocktail { Name = Name }, ingredients);
                }
                else
                {
                    _existingCocktail.Name = Name;
                    await _cocktailService.UpdateCocktailAsync(_existingCocktail, ingredients);
                }

                CloseRequested?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class CocktailIngredientViewModel
    {
        public int IngredientId { get; set; }
        public string IngredientName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
    }
}

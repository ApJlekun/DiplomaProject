using BarInventoryApp.Models;
using BarInventoryApp.Services;
using BarInventoryApp.Utils;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace BarInventoryApp.ViewModels
{
    public class CreateReceiptViewModel : INotifyPropertyChanged
    {
        private readonly IngredientService _ingredientService;
        private readonly ReceiptService _receiptService;
        private readonly CocktailService _cocktailService;
        private string _searchText = string.Empty;
        private decimal _quantity;
        private SearchItem? _selectedItem;
        private bool _isUpdating;

        public ObservableCollection<SearchItem> FoundItems { get; } = new();
        public ObservableCollection<ReceiptItemViewModel> ReceiptItems { get; } = new();

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
                    _selectedItem = null;
                    OnPropertyChanged(nameof(SelectedItem));
                    SearchItems();
                }
            }
        }

        public decimal Quantity
        {
            get => _quantity;
            set { _quantity = value; OnPropertyChanged(); }
        }

        public SearchItem? SelectedItem
        {
            get => _selectedItem;
            set 
            { 
                if (_isUpdating) return;

                if (value == null && _selectedItem != null && _searchText == _selectedItem.Name)
                {
                    return;
                }
                
                _selectedItem = value; 
                if (_selectedItem != null)
                {
                    _isUpdating = true;
                    SearchText = _selectedItem.Name;
                    FoundItems.Clear();
                    _isUpdating = false;
                }
                OnPropertyChanged(); 
            }
        }

        public ICommand AddItemCommand { get; }
        public ICommand RemoveItemCommand { get; }
        public ICommand ProcessCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action? CloseRequested;

        public CreateReceiptViewModel(IngredientService ingredientService, ReceiptService receiptService, CocktailService cocktailService)
        {
            _ingredientService = ingredientService;
            _receiptService = receiptService;
            _cocktailService = cocktailService;

            AddItemCommand = new RelayCommand(OnAddItem);
            RemoveItemCommand = new RelayCommand<ReceiptItemViewModel>(OnRemoveItem);
            ProcessCommand = new RelayCommand(OnProcess);
            CancelCommand = new RelayCommand(OnCancel);
        }

        private void OnRemoveItem(ReceiptItemViewModel? item)
        {
            if (item != null) ReceiptItems.Remove(item);
        }

        private async void SearchItems()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FoundItems.Clear();
                return;
            }

            var ingredientResults = await _ingredientService.SearchAsync(SearchText);
            var cocktailResults = await _cocktailService.SearchAsync(SearchText);
            
            if (!_isUpdating)
            {
                FoundItems.Clear();
                foreach (var item in ingredientResults)
                {
                    FoundItems.Add(new SearchItem 
                    { 
                        IngredientId = item.Id, 
                        Name = item.Name, 
                        Type = "Ингредиент", 
                        Unit = item.Unit,
                        AvailableQuantity = item.Quantity
                    });
                }
                foreach (var item in cocktailResults)
                {
                    FoundItems.Add(new SearchItem 
                    { 
                        CocktailId = item.Id, 
                        Name = item.Name, 
                        Type = "Коктейль", 
                        Unit = "шт.",
                        AvailableQuantity = 999 // Для коктейлей проверка будет при списании
                    });
                }
            }
        }

        private async void OnAddItem()
        {
            if (SelectedItem == null && !string.IsNullOrWhiteSpace(SearchText))
            {
                SearchItems(); // Повторный поиск если нужно
                SelectedItem = FoundItems.FirstOrDefault(i => i.Name.Equals(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            if (SelectedItem == null)
            {
                MessageBox.Show("Выберите позицию из списка.");
                return;
            }

            if (Quantity <= 0)
            {
                MessageBox.Show("Введите корректное количество.");
                return;
            }

            if (SelectedItem.IngredientId.HasValue && SelectedItem.AvailableQuantity < Quantity)
            {
                MessageBox.Show($"Недостаточно ингредиента на остатке. Имеется: {SelectedItem.AvailableQuantity}");
                return;
            }

            ReceiptItems.Add(new ReceiptItemViewModel
            {
                IngredientId = SelectedItem.IngredientId,
                CocktailId = SelectedItem.CocktailId,
                Name = SelectedItem.Name,
                Quantity = Quantity,
                Unit = SelectedItem.Unit
            });

            _isUpdating = true;
            SearchText = string.Empty;
            Quantity = 0;
            _selectedItem = null;
            OnPropertyChanged(nameof(SelectedItem));
            FoundItems.Clear();
            _isUpdating = false;
        }

        private async void OnProcess()
        {
            if (ReceiptItems.Count == 0)
            {
                MessageBox.Show("Добавьте хотя бы одну позицию в чек.");
                return;
            }

            try
            {
                var userId = Session.CurrentUser!.Id;
                var items = ReceiptItems.Select(ri => (ri.IngredientId, ri.CocktailId, ri.Quantity)).ToList();

                await _receiptService.CreateReceiptAsync(userId, items);
                MessageBox.Show("Чек успешно проведен.");
                CloseRequested?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при проведении чека: {ex.Message}");
            }
        }

        private void OnCancel()
        {
            CloseRequested?.Invoke();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class SearchItem
    {
        public int? IngredientId { get; set; }
        public int? CocktailId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal AvailableQuantity { get; set; }
        public override string ToString() => $"{Name} ({Type})";
    }

    public class ReceiptItemViewModel
    {
        public int? IngredientId { get; set; }
        public int? CocktailId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
    }
}

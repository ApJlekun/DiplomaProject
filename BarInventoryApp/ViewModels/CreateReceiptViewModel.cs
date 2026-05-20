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
        private string _searchText = string.Empty;
        private decimal _quantity;
        private Ingredient? _selectedIngredient;
        private bool _isUpdating;

        public ObservableCollection<Ingredient> FoundIngredients { get; } = new();
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
                    // Если пользователь начал печатать что-то новое, сбрасываем выбор
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

                // ЗАЩИТА: Если WPF пытается сбросить выбор в null при скрытии списка,
                // но текст в поле все еще совпадает с текущим выбором — игнорируем сброс.
                if (value == null && _selectedIngredient != null && _searchText == _selectedIngredient.Name)
                {
                    return;
                }
                
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

        public ICommand AddItemCommand { get; }
        public ICommand ProcessCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action? CloseRequested;

        public CreateReceiptViewModel(IngredientService ingredientService, ReceiptService receiptService)
        {
            _ingredientService = ingredientService;
            _receiptService = receiptService;

            AddItemCommand = new RelayCommand(OnAddItem);
            ProcessCommand = new RelayCommand(OnProcess);
            CancelCommand = new RelayCommand(OnCancel);
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
                foreach (var item in results)
                {
                    FoundIngredients.Add(item);
                }
            }
        }

        private async void OnAddItem()
        {
            // Если объект все еще null, пробуем найти его в базе по точному имени (последний шанс)
            if (SelectedIngredient == null && !string.IsNullOrWhiteSpace(SearchText))
            {
                var searchResults = await _ingredientService.SearchAsync(SearchText);
                var exactMatch = searchResults.FirstOrDefault(i => i.Name.Equals(SearchText, StringComparison.OrdinalIgnoreCase));
                if (exactMatch != null)
                {
                    SelectedIngredient = exactMatch;
                }
            }

            if (SelectedIngredient == null)
            {
                MessageBox.Show("Выберите ингредиент из списка.");
                return;
            }

            if (Quantity <= 0)
            {
                MessageBox.Show("Введите корректное количество.");
                return;
            }

            if (SelectedIngredient.Quantity < Quantity)
            {
                MessageBox.Show($"Недостаточно ингредиента на остатке. Имеется: {SelectedIngredient.Quantity}");
                return;
            }

            ReceiptItems.Add(new ReceiptItemViewModel
            {
                IngredientId = SelectedIngredient.Id,
                IngredientName = SelectedIngredient.Name,
                Quantity = Quantity,
                Unit = SelectedIngredient.Unit
            });

            // Сброс полей
            _isUpdating = true;
            SearchText = string.Empty;
            Quantity = 0;
            _selectedIngredient = null;
            OnPropertyChanged(nameof(SelectedIngredient));
            FoundIngredients.Clear();
            _isUpdating = false;
        }

        private async void OnProcess()
        {
            if (ReceiptItems.Count == 0)
            {
                MessageBox.Show("Добавьте хотя бы один ингредиент в чек.");
                return;
            }

            try
            {
                var userId = Session.CurrentUser!.Id;
                var items = ReceiptItems.Select(ri => (ri.IngredientId, ri.Quantity)).ToList();

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

    public class ReceiptItemViewModel
    {
        public int IngredientId { get; set; }
        public string IngredientName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
    }
}

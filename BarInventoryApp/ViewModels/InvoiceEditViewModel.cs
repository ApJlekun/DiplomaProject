using BarInventoryApp.Models;
using BarInventoryApp.Services;
using BarInventoryApp.Utils;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace BarInventoryApp.ViewModels
{
    public class InvoiceEditViewModel : INotifyPropertyChanged
    {
        private readonly Invoice? _invoice;
        private readonly InvoiceService _invoiceService;
        private readonly IngredientService _ingredientService;
        private readonly ExcelExportService _excelService;

        private string _searchText = string.Empty;
        private decimal _quantity;
        private Ingredient? _selectedIngredient;
        private bool _isUpdating;

        public ObservableCollection<Ingredient> FoundIngredients { get; } = new();
        public ObservableCollection<InvoiceItemViewModel> InvoiceItems { get; } = new();

        public string StatusText => _invoice?.Status ?? "Новая";
        public bool IsEditable => _invoice == null || _invoice.Status == "Не проведена";

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

        public ICommand AddItemCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand PostCommand { get; }
        public ICommand ExportCommand { get; }

        public event Action? CloseRequested;

        public InvoiceEditViewModel(Invoice? invoice, InvoiceService invoiceService, IngredientService ingredientService, ExcelExportService excelService)
        {
            _invoice = invoice;
            _invoiceService = invoiceService;
            _ingredientService = ingredientService;
            _excelService = excelService;

            AddItemCommand = new RelayCommand(OnAddItem);
            SaveCommand = new RelayCommand(() => OnSave(false));
            PostCommand = new RelayCommand(() => OnSave(true));
            ExportCommand = new RelayCommand(OnExport);

            LoadInvoiceItems();
        }

        private async void LoadInvoiceItems()
        {
            if (_invoice != null)
            {
                var items = await _invoiceService.GetInvoiceItemsAsync(_invoice.Id);
                foreach (var item in items)
                {
                    InvoiceItems.Add(new InvoiceItemViewModel
                    {
                        IngredientId = item.IngredientId,
                        IngredientName = item.Ingredient.Name,
                        Quantity = item.Quantity,
                        Unit = item.Ingredient.Unit
                    });
                }
            }
        }

        private async void SearchIngredients()
        {
            if (string.IsNullOrWhiteSpace(SearchText)) { FoundIngredients.Clear(); return; }
            var results = await _ingredientService.SearchAsync(SearchText);
            if (!_isUpdating)
            {
                FoundIngredients.Clear();
                foreach (var item in results) FoundIngredients.Add(item);
            }
        }

        private async void OnAddItem()
        {
            if (SelectedIngredient == null && !string.IsNullOrWhiteSpace(SearchText))
            {
                var searchResults = await _ingredientService.SearchAsync(SearchText);
                var exactMatch = searchResults.FirstOrDefault(i => i.Name.Equals(SearchText, StringComparison.OrdinalIgnoreCase));
                if (exactMatch != null) SelectedIngredient = exactMatch;
            }

            if (SelectedIngredient == null) { MessageBox.Show("Выберите ингредиент."); return; }
            if (Quantity <= 0) { MessageBox.Show("Введите количество."); return; }

            InvoiceItems.Add(new InvoiceItemViewModel
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

        private async void OnSave(bool post)
        {
            if (InvoiceItems.Count == 0) { MessageBox.Show("Добавьте позиции."); return; }

            try
            {
                var invoice = _invoice ?? new Invoice { CreatedBy = Session.CurrentUser!.Id, CreatedAt = DateTime.UtcNow };
                var items = InvoiceItems.Select(i => (i.IngredientId, i.Quantity)).ToList();

                await _invoiceService.SaveInvoiceAsync(invoice, items, post);
                MessageBox.Show(post ? "Накладная проведена." : "Накладная сохранена.");
                CloseRequested?.Invoke();
            }
            catch (Exception ex) { MessageBox.Show($"Ошибка: {ex.Message}"); }
        }

        private void OnExport()
        {
            try
            {
                var data = InvoiceItems.Select(i => new { Название = i.IngredientName, Количество = i.Quantity, ЕдИзм = i.Unit }).ToList();
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    FileName = $"Invoice_{(_invoice?.Id.ToString() ?? "New")}_{DateTime.Now:yyyyMMdd}"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    _excelService.ExportToExcel(data, saveFileDialog.FileName);
                    MessageBox.Show("Экспорт завершен.");
                }
            }
            catch (Exception ex) { MessageBox.Show($"Ошибка экспорта: {ex.Message}"); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class InvoiceItemViewModel
    {
        public int IngredientId { get; set; }
        public string IngredientName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
    }
}

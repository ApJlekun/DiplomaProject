using BarInventoryApp.Constants;
using BarInventoryApp.Models;
using BarInventoryApp.Pages;
using BarInventoryApp.Services;
using BarInventoryApp.Utils;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BarInventoryApp.ViewModels;

/// <summary>
/// ViewModel для страницы управления ингредиентами (теперь сгруппированными по категориям).
/// </summary>
public class IngredientsViewModel : INotifyPropertyChanged
{
    #region Поля

    private readonly IngredientService _service;
    private readonly ReceiptService _receiptService;
    private readonly CocktailService _cocktailService;
    private readonly MainViewModel _mainViewModel;
    private readonly IServiceProvider _serviceProvider;
    private readonly ExcelImportService _importService;
    private readonly RevisionService _revisionService;
    private readonly ExcelExportService _excelService;
    private string _filter = string.Empty;
    private List<Category> _allCategories = new();
    private Category? _selectedCategory;
    private Ingredient? _selectedSearchIngredient;
    private Ingredient? _selectedIngredient;
    private bool _isSearchDropDownOpen;

    #endregion

    #region Свойства

    /// <summary>
    /// Коллекция отфильтрованных категорий для отображения.
    /// </summary>
    public ObservableCollection<Category> Categories { get; } = new();

    /// <summary>
    /// Коллекция найденных ингредиентов для выпадающего списка поиска.
    /// </summary>
    public ObservableCollection<Ingredient> FilteredIngredients { get; } = new();

    /// <summary>
    /// Текст фильтра для поиска ингредиентов.
    /// </summary>
    public string Filter
    {
        get => _filter;
        set 
        { 
            if (_filter == value) return;
            _filter = value; 

            ApplyIngredientFilter();

            // Просто устанавливаем флаг открытия. 
            // Обработчик OnSearchTextChanged в code-behind позаботится о курсоре.
            IsSearchDropDownOpen = FilteredIngredients.Count > 0 && !string.IsNullOrEmpty(value);
        }
    }

    /// <summary>
    /// Состояние выпадающего списка поиска.
    /// </summary>
    public bool IsSearchDropDownOpen
    {
        get => _isSearchDropDownOpen;
        set 
        { 
            if (_isSearchDropDownOpen == value) return;
            _isSearchDropDownOpen = value; 
            OnPropertyChanged(); 
        }
    }

    /// <summary>
    /// Выбранный ингредиент из результатов поиска.
    /// </summary>
    public Ingredient? SelectedSearchIngredient
    {
        get => _selectedSearchIngredient;
        set
        {
            if (_selectedSearchIngredient == value) return;
            _selectedSearchIngredient = value;
            OnPropertyChanged();

            if (_selectedSearchIngredient != null)
            {
                FocusOnIngredient(_selectedSearchIngredient);
                IsSearchDropDownOpen = false;
                _filter = string.Empty;
                OnPropertyChanged(nameof(Filter));
            }
        }
    }

    /// <summary>
    /// Выбранный ингредиент в раскрытом списке категории.
    /// </summary>
    public Ingredient? SelectedIngredient
    {
        get => _selectedIngredient;
        set { _selectedIngredient = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Выбранная категория в списке.
    /// </summary>
    public Category? SelectedCategory
    {
        get => _selectedCategory;
        set { _selectedCategory = value; OnPropertyChanged(); }
    }

    #endregion

    #region Команды

    public ICommand? AddCommand { get; }
    public ICommand? EditCommand { get; }
    public ICommand? DeleteCommand { get; }
    public ICommand? ImportCommand { get; }
    public ICommand? CreateReceiptCommand { get; }
    public ICommand? ReceiptHistoryCommand { get; }
    public ICommand? RevisionCommand { get; }
    public ICommand? RevisionHistoryCommand { get; }
    public ICommand LowStockCommand { get; }

    #endregion

    #region Конструктор

    public IngredientsViewModel(IngredientService service, ReceiptService receiptService, CocktailService cocktailService, MainViewModel mainViewModel, IServiceProvider serviceProvider, ExcelImportService importService, RevisionService revisionService, ExcelExportService excelService)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _receiptService = receiptService ?? throw new ArgumentNullException(nameof(receiptService));
        _cocktailService = cocktailService ?? throw new ArgumentNullException(nameof(cocktailService));
        _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _importService = importService ?? throw new ArgumentNullException(nameof(importService));
        _revisionService = revisionService ?? throw new ArgumentNullException(nameof(revisionService));
        _excelService = excelService ?? throw new ArgumentNullException(nameof(excelService));

        var role = Session.CurrentUser?.Role.Name;
        bool isManagerOrAdmin = role == ApplicationConstants.Roles.Admin || role == ApplicationConstants.Roles.Manager;

        if (role == ApplicationConstants.Roles.Admin)
        {
            AddCommand = new RelayCommand(OnAdd);
            ImportCommand = new RelayCommand(OnImport);
        }

        if (isManagerOrAdmin)
        {
            CreateReceiptCommand = new RelayCommand(OnCreateReceipt);
            ReceiptHistoryCommand = new RelayCommand(OnReceiptHistory);
            DeleteCommand = new RelayCommand<Ingredient>(OnDeleteIngredient);
            EditCommand = new RelayCommand<Ingredient>(OnEditIngredient);
            RevisionCommand = new RelayCommand(OnRevision);
            RevisionHistoryCommand = new RelayCommand(OnRevisionHistory);
        }

        LowStockCommand = new RelayCommand(OnLowStock);

        LoadData();
    }

    #endregion

    #region Методы

    private async void LoadData()
    {
        // Запоминаем текущий выбор, чтобы восстановить его после обновления
        int? selectedCategoryId = SelectedCategory?.Id;
        int? selectedIngredientId = SelectedIngredient?.Id;

        try
        {
            _allCategories = await _service.GetCategoriesAsync();
            ApplyFilter();

            // Восстанавливаем выбор категории (это раскроет её список)
            if (selectedCategoryId.HasValue)
            {
                var categoryToRestore = Categories.FirstOrDefault(c => c.Id == selectedCategoryId.Value);
                if (categoryToRestore != null)
                {
                    SelectedCategory = categoryToRestore;

                    // Если возможно, восстанавливаем и выбор конкретного ингредиента
                    if (selectedIngredientId.HasValue)
                    {
                        SelectedIngredient = SelectedCategory.Ingredients.FirstOrDefault(i => i.Id == selectedIngredientId.Value);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ApplyFilter()
    {
        Categories.Clear();
        foreach (var category in _allCategories)
        {
            Categories.Add(category);
        }
    }

    private void ApplyIngredientFilter()
    {
        FilteredIngredients.Clear();
        if (string.IsNullOrWhiteSpace(Filter)) return;

        var matches = _allCategories
            .SelectMany(c => c.Ingredients)
            .Where(i => i.Name.Contains(Filter, StringComparison.OrdinalIgnoreCase))
            .Take(10)
            .ToList();

        foreach (var ingredient in matches)
        {
            FilteredIngredients.Add(ingredient);
        }
    }

    private void FocusOnIngredient(Ingredient ingredient)
    {
        var category = Categories.FirstOrDefault(c => c.Id == ingredient.CategoryId);
        if (category != null)
        {
            SelectedCategory = category;
            SelectedIngredient = ingredient;
        }
    }

    private void OnCreateReceipt()
    {
        var viewModel = new CreateReceiptViewModel(_service, _receiptService, _cocktailService);
        var dialog = new CreateReceiptDialog(viewModel);
        dialog.ShowDialog();
        LoadData();
    }

    private void OnReceiptHistory()
    {
        var viewModel = new ReceiptHistoryViewModel(_receiptService);
        var dialog = new ReceiptHistoryDialog(viewModel);
        dialog.ShowDialog();
    }

    private void OnRevision()
    {
        var viewModel = new RevisionViewModel(_revisionService, _service, _excelService);
        var dialog = new RevisionDialog(viewModel);
        dialog.ShowDialog();
        LoadData();
    }

    private void OnRevisionHistory()
    {
        var viewModel = new RevisionHistoryViewModel(_revisionService, _service, _excelService, _serviceProvider);
        var dialog = new RevisionHistoryDialog(viewModel);
        dialog.ShowDialog();
        LoadData();
    }

    private void OnLowStock()
    {
        var viewModel = new LowStockIngredientsViewModel(_service);
        var dialog = new LowStockIngredientsDialog(viewModel);
        dialog.ShowDialog();
        LoadData();
    }
    private void OnAdd()
    {
        var dialog = new IngredientEditDialog(null, _service);
        if (dialog.ShowDialog() == true) LoadData();
    }

    private void OnEditIngredient(Ingredient? ingredient)
    {
        if (ingredient == null)
        {
            MessageBox.Show("Выберите ингредиент для редактирования.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var dialog = new IngredientEditDialog(ingredient, _service);
        if (dialog.ShowDialog() == true)
        {
            LoadData();
        }
    }

    private void OnEdit()
    {
        MessageBox.Show("Для редактирования ингредиента откройте соответствующую категорию и используйте поиск.");
    }

    private async void OnDeleteIngredient(Ingredient ingredient)
    {
        if (ingredient == null) return;

        var result = MessageBox.Show($"Вы уверены, что хотите удалить ингредиент \"{ingredient.Name}\"?", 
            "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                await _service.DeleteAsync(ingredient.Id);
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void OnDelete()
    {
        // Старый метод без параметров, заменен на OnDeleteIngredient
    }

    private async void OnImport()
    {
        try
        {
            _importService.ShowImportInstructions();
            var importedIngredients = _importService.ImportIngredients();
            if (importedIngredients == null || importedIngredients.Count == 0) return;

            int addedCount = 0;
            int updatedCount = 0;

            var allIngredients = await _service.GetAllAsync();
            var allCategories = await _service.GetCategoriesAsync();

            foreach (var importedIngredient in importedIngredients)
            {
                var categoryName = importedIngredient.Category?.Name ?? "Без категории";
                var category = allCategories.FirstOrDefault(c => c.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
                
                if (category == null)
                {
                    category = await _service.AddCategoryAsync(new Category { Name = categoryName });
                    allCategories.Add(category);
                }

                importedIngredient.CategoryId = category.Id;
                importedIngredient.Category = null!; // Avoid EF tracking issues with new instances

                var existingIngredient = allIngredients.FirstOrDefault(i =>
                    i.Name.Equals(importedIngredient.Name, StringComparison.OrdinalIgnoreCase));

                if (existingIngredient != null)
                {
                    existingIngredient.Quantity = importedIngredient.Quantity;
                    existingIngredient.Unit = importedIngredient.Unit;
                    existingIngredient.CategoryId = category.Id;
                    await _service.UpdateAsync(existingIngredient);
                    updatedCount++;
                }
                else
                {
                    await _service.AddAsync(importedIngredient);
                    addedCount++;
                }
            }

            LoadData();
            MessageBox.Show($"Импорт завершен!\nДобавлено: {addedCount}\nОбновлено: {updatedCount}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка импорта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Переходит на указанную страницу через MainViewModel.
    /// </summary>
    public void NavigateTo<T>() where T : Page => _mainViewModel.NavigateTo<T>();

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

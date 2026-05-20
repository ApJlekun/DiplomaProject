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
    private readonly MainViewModel _mainViewModel;
    private readonly IServiceProvider _serviceProvider;
    private readonly ExcelImportService _importService;
    private string _filter = string.Empty;
    private List<Category> _allCategories = new();
    private Category? _selectedCategory;

    #endregion

    #region Свойства

    /// <summary>
    /// Коллекция отфильтрованных категорий для отображения.
    /// </summary>
    public ObservableCollection<Category> Categories { get; } = new();

    /// <summary>
    /// Текст фильтра для поиска категорий.
    /// </summary>
    public string Filter
    {
        get => _filter;
        set { _filter = value; OnPropertyChanged(); ApplyFilter(); }
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
    public ICommand? OpenCategoryCommand { get; }

    #endregion

    #region Конструктор

    public IngredientsViewModel(IngredientService service, ReceiptService receiptService, MainViewModel mainViewModel, IServiceProvider serviceProvider, ExcelImportService importService)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _receiptService = receiptService ?? throw new ArgumentNullException(nameof(receiptService));
        _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _importService = importService ?? throw new ArgumentNullException(nameof(importService));

        var role = Session.CurrentUser?.Role.Name;
        bool isManagerOrAdmin = role == ApplicationConstants.Roles.Admin || role == ApplicationConstants.Roles.Manager;

        if (role == ApplicationConstants.Roles.Admin)
        {
            AddCommand = new RelayCommand(OnAdd);
            EditCommand = new RelayCommand(OnEdit);
            DeleteCommand = new RelayCommand(OnDelete);
            ImportCommand = new RelayCommand(OnImport);
        }

        if (isManagerOrAdmin)
        {
            CreateReceiptCommand = new RelayCommand(OnCreateReceipt);
            ReceiptHistoryCommand = new RelayCommand(OnReceiptHistory);
        }

        OpenCategoryCommand = new RelayCommand(OnOpenCategory);

        LoadData();
    }

    #endregion

    #region Методы

    private async void LoadData()
    {
        try
        {
            _allCategories = await _service.GetCategoriesAsync();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ApplyFilter()
    {
        var filtered = _allCategories
            .Where(c => string.IsNullOrEmpty(Filter) ||
                        c.Name.Contains(Filter, StringComparison.OrdinalIgnoreCase))
            .ToList();

        Categories.Clear();
        foreach (var category in filtered)
        {
            Categories.Add(category);
        }
    }

    private async void OnOpenCategory()
    {
        if (SelectedCategory == null) return;

        // Загружаем ингредиенты этой категории
        var allIngredients = await _service.GetAllAsync();
        var categoryIngredients = allIngredients.Where(i => i.CategoryId == SelectedCategory.Id).ToList();

        var viewModel = new CategoryIngredientsViewModel(SelectedCategory.Name, categoryIngredients);
        var dialog = new CategoryIngredientsDialog(viewModel);
        dialog.ShowDialog();
    }

    private void OnCreateReceipt()
    {
        var viewModel = new CreateReceiptViewModel(_service, _receiptService);
        var dialog = new CreateReceiptDialog(viewModel);
        dialog.ShowDialog();
        LoadData(); // На всякий случай обновляем (хотя количество в диалоге категории)
    }

    private void OnReceiptHistory()
    {
        var viewModel = new ReceiptHistoryViewModel(_receiptService);
        var dialog = new ReceiptHistoryDialog(viewModel);
        dialog.ShowDialog();
    }

    private void OnAdd()
    {
        var dialog = new IngredientEditDialog(null, _service);
        if (dialog.ShowDialog() == true) LoadData();
    }

    private void OnEdit()
    {
        MessageBox.Show("Для редактирования ингредиента откройте соответствующую категорию двойным кликом (функционал в разработке) или используйте поиск.");
        // В текущей реализации по категориям редактирование усложнено, 
        // обычно оно делается через поиск или внутри списка категории.
    }

    private void OnDelete()
    {
        MessageBox.Show("Удаление ингредиентов производится из списка внутри категории.");
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

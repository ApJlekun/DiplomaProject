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

namespace BarInventoryApp.ViewModels;

public class RevisionViewModel : INotifyPropertyChanged
{
    private readonly RevisionService _revisionService;
    private readonly IngredientService _ingredientService;
    private readonly ExcelExportService _excelService;
    private Revision _revision;
    private bool _isReadOnly;

    public Revision Revision
    {
        get => _revision;
        set { _revision = value; OnPropertyChanged(); }
    }

    public bool IsReadOnly
    {
        get => _isReadOnly;
        set { _isReadOnly = value; OnPropertyChanged(); }
    }

    public ObservableCollection<RevisionItemViewModel> Items { get; } = new();

    public ICommand SaveCommand { get; }
    public ICommand ProcessCommand { get; }
    public ICommand ExportCommand { get; }

    public RevisionViewModel(RevisionService revisionService, IngredientService ingredientService, ExcelExportService excelService, Revision? revision = null)
    {
        _revisionService = revisionService;
        _ingredientService = ingredientService;
        _excelService = excelService;

        if (revision == null)
        {
            _revision = new Revision
            {
                CreatedByUserId = Session.CurrentUser?.Id ?? 0,
                Status = "Новая",
                CreatedAt = DateTime.UtcNow
            };
            LoadNewItems();
        }
        else
        {
            _revision = revision;
            _isReadOnly = revision.ProcessedAt.HasValue;
            foreach (var item in revision.RevisionItems)
            {
                Items.Add(new RevisionItemViewModel(item, _isReadOnly));
            }
        }

        SaveCommand = new RelayCommand(OnSave, () => !IsReadOnly);
        ProcessCommand = new RelayCommand(OnProcess, () => !IsReadOnly);
        ExportCommand = new RelayCommand(OnExport);
    }

    private async void LoadNewItems()
    {
        var ingredients = await _ingredientService.GetAllAsync();
        foreach (var ing in ingredients)
        {
            var item = new RevisionItem
            {
                IngredientId = ing.Id,
                Ingredient = ing,
                SystemQuantity = ing.Quantity,
                ActualQuantity = ing.Quantity, // По дефолту равно системному
                Discrepancy = 0
            };
            Items.Add(new RevisionItemViewModel(item, false));
        }
    }

    private async void OnSave()
    {
        try
        {
            UpdateRevisionModel();
            await _revisionService.SaveAsync(_revision);
            MessageBox.Show("Ревизия успешно сохранена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void OnProcess()
    {
        var result = MessageBox.Show("Вы уверены, что хотите провести ревизию? Это изменит остатки в системе и запись будет законсервирована.", 
            "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                UpdateRevisionModel();
                await _revisionService.ProcessAsync(_revision);
                IsReadOnly = true;
                foreach (var item in Items) item.IsReadOnly = true;
                MessageBox.Show("Ревизия успешно проведена. Остатки обновлены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при проведении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void OnExport()
    {
        try
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"Revision_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var exportData = Items.Select(i => new
                {
                    Наименование = i.Name,
                    Система = i.SystemQuantity,
                    Ед_Изм = i.Unit,
                    Факт = i.ActualQuantity,
                    Расхождение = i.Discrepancy
                }).ToList();

                _excelService.ExportToExcel(exportData, saveFileDialog.FileName);
                MessageBox.Show("Экспорт успешно завершен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void UpdateRevisionModel()
    {
        _revision.RevisionItems = Items.Select(i => i.Model).ToList();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class RevisionItemViewModel : INotifyPropertyChanged
{
    public RevisionItem Model { get; }
    private bool _isReadOnly;

    public string Name => Model.Ingredient?.Name ?? "Неизвестно";
    public decimal SystemQuantity => Model.SystemQuantity;
    public string Unit => Model.Ingredient?.Unit ?? "";

    public decimal ActualQuantity
    {
        get => Model.ActualQuantity;
        set
        {
            if (_isReadOnly) return;
            Model.ActualQuantity = value;
            Model.Discrepancy = Model.ActualQuantity - Model.SystemQuantity;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Discrepancy));
            OnPropertyChanged(nameof(DiscrepancyColor));
        }
    }

    public decimal Discrepancy => Model.Discrepancy;

    public string DiscrepancyColor
    {
        get
        {
            if (Discrepancy < 0) return "Red";
            if (Discrepancy > 0) return "Green";
            return "White"; // Или стандартный цвет текста
        }
    }

    public bool IsReadOnly
    {
        get => _isReadOnly;
        set { _isReadOnly = value; OnPropertyChanged(); }
    }

    public RevisionItemViewModel(RevisionItem model, bool isReadOnly)
    {
        Model = model;
        _isReadOnly = isReadOnly;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

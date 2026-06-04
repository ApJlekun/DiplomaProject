using BarInventoryApp.Models;
using BarInventoryApp.Pages;
using BarInventoryApp.Services;
using BarInventoryApp.Utils;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace BarInventoryApp.ViewModels;

public class RevisionHistoryViewModel : INotifyPropertyChanged
{
    private readonly RevisionService _revisionService;
    private readonly IngredientService _ingredientService;
    private readonly ExcelExportService _excelService;
    private readonly IServiceProvider _serviceProvider;
    private Revision? _selectedRevision;

    public ObservableCollection<Revision> Revisions { get; } = new();

    public Revision? SelectedRevision
    {
        get => _selectedRevision;
        set { _selectedRevision = value; OnPropertyChanged(); }
    }

    public ICommand OpenCommand { get; }

    public RevisionHistoryViewModel(RevisionService revisionService, IngredientService ingredientService, ExcelExportService excelService, IServiceProvider serviceProvider)
    {
        _revisionService = revisionService;
        _ingredientService = ingredientService;
        _excelService = excelService;
        _serviceProvider = serviceProvider;

        OpenCommand = new RelayCommand(OnOpen, () => SelectedRevision != null);
        LoadData();
    }

    private async void LoadData()
    {
        try
        {
            var revisions = await _revisionService.GetAllAsync();
            Revisions.Clear();
            foreach (var rev in revisions)
            {
                Revisions.Add(rev);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки истории ревизий: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void OnOpen()
    {
        if (SelectedRevision == null) return;

        try
        {
            var detailedRevision = await _revisionService.GetByIdAsync(SelectedRevision.Id);
            if (detailedRevision == null) return;

            var viewModel = new RevisionViewModel(_revisionService, _ingredientService, _excelService, detailedRevision);
            var dialog = new RevisionDialog(viewModel);
            dialog.ShowDialog();
            
            LoadData(); // Обновляем историю после закрытия окна ревизии
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при открытии ревизии: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

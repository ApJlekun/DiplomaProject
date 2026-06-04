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
    public class ReceiptDetailsViewModel : INotifyPropertyChanged
    {
        private readonly int _receiptId;
        private readonly ReceiptService _receiptService;
        private readonly ExcelExportService _excelService;

        public ObservableCollection<ReceiptItem> Items { get; } = new();

        public ICommand ExportToExcelCommand { get; }

        public ReceiptDetailsViewModel(int receiptId, ReceiptService receiptService, ExcelExportService excelService)
        {
            _receiptId = receiptId;
            _receiptService = receiptService;
            _excelService = excelService;
            ExportToExcelCommand = new RelayCommand(OnExportToExcel);
            LoadDetails();
        }

        private async void LoadDetails()
        {
            var items = await _receiptService.GetReceiptItemsAsync(_receiptId);
            Items.Clear();
            foreach (var item in items)
            {
                Items.Add(item);
            }
        }

        private void OnExportToExcel()
        {
            if (Items.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var sfd = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                FileName = $"Receipt_{_receiptId}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    var exportData = Items.Select(i => new
                    {
                        Наименование = i.DisplayName,
                        Количество = i.Quantity,
                        Ед_Изм = i.DisplayUnit
                    }).ToList();

                    _excelService.ExportToExcel(exportData, sfd.FileName);
                    MessageBox.Show("Экспорт чека успешно завершен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

using BarInventoryApp.Models;
using BarInventoryApp.Services;
using BarInventoryApp.Utils;
using BarInventoryApp.Pages;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Linq;

namespace BarInventoryApp.ViewModels
{
    public class ReceiptHistoryViewModel : INotifyPropertyChanged
    {
        private readonly ReceiptService _receiptService;
        private Receipt? _selectedReceipt;

        public ObservableCollection<Receipt> Receipts { get; } = new();

        public Receipt? SelectedReceipt
        {
            get => _selectedReceipt;
            set { _selectedReceipt = value; OnPropertyChanged(); }
        }

        public ICommand ShowDetailsCommand { get; }
        public ICommand CloseCommand { get; }

        public event Action? CloseRequested;

        public ReceiptHistoryViewModel(ReceiptService receiptService)
        {
            _receiptService = receiptService;
            ShowDetailsCommand = new RelayCommand(OnShowDetails);
            CloseCommand = new RelayCommand(() => CloseRequested?.Invoke());
            LoadHistory();
        }

        private async void LoadHistory()
        {
            var receipts = await _receiptService.GetReceiptsAsync();
            Receipts.Clear();
            foreach (var r in receipts)
            {
                Receipts.Add(r);
            }
        }

        private void OnShowDetails()
        {
            if (SelectedReceipt == null) return;

            var excelService = App.ServiceProvider.GetService(typeof(ExcelExportService)) as ExcelExportService;
            var detailsViewModel = new ReceiptDetailsViewModel(SelectedReceipt.Id, _receiptService, excelService!);
            var dialog = new ReceiptDetailsDialog(detailsViewModel);
            dialog.ShowDialog();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

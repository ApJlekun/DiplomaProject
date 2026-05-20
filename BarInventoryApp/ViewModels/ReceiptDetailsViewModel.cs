using BarInventoryApp.Models;
using BarInventoryApp.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BarInventoryApp.ViewModels
{
    public class ReceiptDetailsViewModel : INotifyPropertyChanged
    {
        private readonly int _receiptId;
        private readonly ReceiptService _receiptService;

        public ObservableCollection<ReceiptItem> Items { get; } = new();

        public ReceiptDetailsViewModel(int receiptId, ReceiptService receiptService)
        {
            _receiptId = receiptId;
            _receiptService = receiptService;
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

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

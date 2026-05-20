using BarInventoryApp.Constants;
using BarInventoryApp.Models;
using BarInventoryApp.Pages;
using BarInventoryApp.Services;
using BarInventoryApp.Utils;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace BarInventoryApp.ViewModels
{
    public class OrdersViewModel : INotifyPropertyChanged
    {
        private readonly InvoiceService _invoiceService;
        private readonly IngredientService _ingredientService;
        private readonly ExcelExportService _excelService;
        private readonly EmailService _emailService;
        private readonly MainViewModel _mainViewModel;
        private Invoice? _selectedInvoice;
        private string _searchQuery = string.Empty;
        private List<Invoice> _allInvoices = new();

        public ObservableCollection<Invoice> Invoices { get; } = new();

        public Invoice? SelectedInvoice
        {
            get => _selectedInvoice;
            set { _selectedInvoice = value; OnPropertyChanged(); }
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set { _searchQuery = value; OnPropertyChanged(); ApplyFilter(); }
        }

        public bool IsManagerOrAdmin => Session.CurrentUser?.Role.Name == ApplicationConstants.Roles.Admin || 
                                        Session.CurrentUser?.Role.Name == ApplicationConstants.Roles.Manager;

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand SendEmailCommand { get; }

        public OrdersViewModel(InvoiceService invoiceService, IngredientService ingredientService, ExcelExportService excelService, EmailService emailService, MainViewModel mainViewModel)
        {
            _invoiceService = invoiceService;
            _ingredientService = ingredientService;
            _excelService = excelService;
            _emailService = emailService;
            _mainViewModel = mainViewModel;

            AddCommand = new RelayCommand(OnAdd);
            EditCommand = new RelayCommand(OnEdit);
            DeleteCommand = new RelayCommand(OnDelete);
            BackCommand = new RelayCommand(OnBack);
            SendEmailCommand = new RelayCommand(OnSendEmail);

            LoadInvoices();
        }

        private async void LoadInvoices()
        {
            _allInvoices = await _invoiceService.GetAllInvoicesAsync();
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            var filtered = _allInvoices.Where(i => string.IsNullOrEmpty(SearchQuery) || 
                                                   i.Id.ToString().Contains(SearchQuery)).ToList();
            Invoices.Clear();
            foreach (var item in filtered) Invoices.Add(item);
        }

        private void OnAdd()
        {
            var viewModel = new InvoiceEditViewModel(null, _invoiceService, _ingredientService, _excelService);
            var dialog = new InvoiceEditDialog(viewModel);
            viewModel.CloseRequested += () => { dialog.DialogResult = true; dialog.Close(); };
            if (dialog.ShowDialog() == true) LoadInvoices();
        }

        private void OnEdit()
        {
            if (SelectedInvoice == null) return;
            var viewModel = new InvoiceEditViewModel(SelectedInvoice, _invoiceService, _ingredientService, _excelService);
            var dialog = new InvoiceEditDialog(viewModel);
            viewModel.CloseRequested += () => { dialog.DialogResult = true; dialog.Close(); };
            if (dialog.ShowDialog() == true) LoadInvoices();
        }

        private async void OnDelete()
        {
            if (SelectedInvoice == null) return;
            if (SelectedInvoice.Status == "Проведена")
            {
                MessageBox.Show("Нельзя удалить проведенную накладную.");
                return;
            }

            var res = MessageBox.Show($"Удалить накладную №{SelectedInvoice.Id}?", "Подтверждение", MessageBoxButton.YesNo);
            if (res == MessageBoxResult.Yes)
            {
                await _invoiceService.DeleteInvoiceAsync(SelectedInvoice.Id);
                LoadInvoices();
            }
        }

        private async void OnSendEmail()
        {
            if (SelectedInvoice == null)
            {
                MessageBox.Show("Выберите накладную для отправки.");
                return;
            }

            if (SelectedInvoice.Status == "Проведена")
            {
                MessageBox.Show("Нельзя отправить проведенную накладную. Отправка доступна только для не проведенных накладных.");
                return;
            }

            var dialog = new SendEmailDialog { Owner = Application.Current.MainWindow };
            if (dialog.ShowDialog() == true)
            {
                string tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"Invoice_{SelectedInvoice.Id}.xlsx");
                try
                {
                    // Подготовка данных для экспорта
                    var items = await _invoiceService.GetInvoiceItemsAsync(SelectedInvoice.Id);
                    var exportData = items.Select(ii => new
                    {
                        Ингредиент = ii.Ingredient.Name,
                        Количество = ii.Quantity,
                        ЕдИзм = ii.Ingredient.Unit
                    }).ToList();

                    _excelService.ExportToExcel(exportData, tempPath);

                    await _emailService.SendEmailWithAttachmentAsync(
                        dialog.RecipientEmail,
                        $"Накладная №{SelectedInvoice.Id} - Bar Inventory",
                        $"Здравствуйте!\n\nВо вложении находится накладная №{SelectedInvoice.Id} от {SelectedInvoice.CreatedAt:dd.MM.yyyy HH:mm}.\n\nС уважением,\nBar Inventory System",
                        tempPath
                    );

                    MessageBox.Show($"Накладная успешно отправлена на {dialog.RecipientEmail}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка при отправке почты: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    if (System.IO.File.Exists(tempPath))
                    {
                        try { System.IO.File.Delete(tempPath); } catch { }
                    }
                }
            }
        }

        private void OnBack()
        {
            var role = Session.CurrentUser?.Role.Name;
            if (role == ApplicationConstants.Roles.Admin) _mainViewModel.NavigateTo<AdminDashboardPage>();
            else if (role == ApplicationConstants.Roles.Manager) _mainViewModel.NavigateTo<ManagerDashboardPage>();
            else _mainViewModel.NavigateTo<AuthorizationPage>();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

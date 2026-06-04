using BarInventoryApp.Models;
using BarInventoryApp.Services;
using BarInventoryApp.Utils;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;

namespace BarInventoryApp.ViewModels
{
    public class LowStockIngredientsViewModel : INotifyPropertyChanged
    {
        private readonly IngredientService _ingredientService;

        public ObservableCollection<LowStockIngredientDisplay> LowStockIngredients { get; } = new();

        public ICommand CloseCommand { get; }
        public event Action? CloseRequested;

        public LowStockIngredientsViewModel(IngredientService ingredientService)
        {
            _ingredientService = ingredientService;
            CloseCommand = new RelayCommand(() => CloseRequested?.Invoke());
            LoadLowStockIngredients();
        }

        private async void LoadLowStockIngredients()
        {
            var allIngredients = await _ingredientService.GetAllAsync();
            var lowStock = allIngredients
                .Where(i => i.Quantity < 10)
                .OrderBy(i => i.Quantity)
                .Select(i => new LowStockIngredientDisplay
                {
                    Name = i.Name,
                    Quantity = i.Quantity,
                    Unit = i.Unit,
                    Color = GetColorForQuantity(i.Quantity)
                });

            LowStockIngredients.Clear();
            foreach (var item in lowStock)
            {
                LowStockIngredients.Add(item);
            }
        }

        private SolidColorBrush GetColorForQuantity(decimal quantity)
        {
            // Спектр от красного (0) до желтого (10)
            double factor = (double)Math.Min(Math.Max(quantity, 0), 10) / 10.0;
            
            // factor 0 -> Red (255, 0, 0)
            // factor 1 -> Yellow (255, 255, 0)
            
            byte r = 255;
            byte g = (byte)(255 * factor);
            byte b = 0;

            return new SolidColorBrush(Color.FromRgb(r, g, b));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class LowStockIngredientDisplay
    {
        public string Name { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
        public SolidColorBrush Color { get; set; } = Brushes.Transparent;
    }
}

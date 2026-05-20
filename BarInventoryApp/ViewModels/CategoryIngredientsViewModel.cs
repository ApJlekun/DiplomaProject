using BarInventoryApp.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace BarInventoryApp.ViewModels
{
    public class CategoryIngredientsViewModel : INotifyPropertyChanged
    {
        private string _filter = string.Empty;
        private readonly List<Ingredient> _allIngredients;

        public string CategoryName { get; }
        public ObservableCollection<Ingredient> Ingredients { get; } = new();

        public string Filter
        {
            get => _filter;
            set { _filter = value; OnPropertyChanged(); ApplyFilter(); }
        }

        public CategoryIngredientsViewModel(string categoryName, List<Ingredient> ingredients)
        {
            CategoryName = categoryName;
            _allIngredients = ingredients ?? new List<Ingredient>();
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            var filtered = _allIngredients
                .Where(i => string.IsNullOrEmpty(Filter) ||
                            i.Name.Contains(Filter, System.StringComparison.OrdinalIgnoreCase))
                .ToList();

            Ingredients.Clear();
            foreach (var item in filtered)
            {
                Ingredients.Add(item);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

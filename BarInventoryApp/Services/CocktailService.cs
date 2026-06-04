using BarInventoryApp.DataContexts;
using BarInventoryApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BarInventoryApp.Services
{
    public class CocktailService
    {
        private readonly AppDbContext _context;

        public CocktailService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Cocktail>> GetAllCocktailsAsync()
        {
            return await _context.Cocktails
                .Include(c => c.CocktailIngredients)
                .ThenInclude(ci => ci.Ingredient)
                .ToListAsync();
        }

        public async Task<Cocktail?> GetCocktailByIdAsync(int id)
        {
            return await _context.Cocktails
                .Include(c => c.CocktailIngredients)
                .ThenInclude(ci => ci.Ingredient)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Cocktail>> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return new List<Cocktail>();
            return await _context.Cocktails
                .Where(c => c.Name.Contains(query))
                .ToListAsync();
        }

        public async Task AddCocktailAsync(Cocktail cocktail, List<CocktailIngredient> ingredients)
        {
            cocktail.CocktailIngredients = ingredients;
            _context.Cocktails.Add(cocktail);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCocktailAsync(Cocktail cocktail, List<CocktailIngredient> ingredients)
        {
            var existing = await _context.Cocktails
                .Include(c => c.CocktailIngredients)
                .FirstOrDefaultAsync(c => c.Id == cocktail.Id);

            if (existing == null) return;

            existing.Name = cocktail.Name;

            // Обновляем ингредиенты: удаляем старые и добавляем новые
            _context.CocktailIngredients.RemoveRange(existing.CocktailIngredients);
            foreach (var ci in ingredients)
            {
                ci.CocktailId = existing.Id;
                _context.CocktailIngredients.Add(ci);
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteCocktailAsync(int id)
        {
            var cocktail = await _context.Cocktails.FindAsync(id);
            if (cocktail != null)
            {
                _context.Cocktails.Remove(cocktail);
                await _context.SaveChangesAsync();
            }
        }
    }
}

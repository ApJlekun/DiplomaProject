using BarInventoryApp.DataContexts;
using BarInventoryApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BarInventoryApp.Services;

/// <summary>
/// Сервис для работы с ингредиентами.
/// </summary>
public class IngredientService
{
    #region Поля

    private readonly AppDbContext _context;

    #endregion

    #region Конструктор

    /// <summary>
    /// Инициализирует новый экземпляр класса IngredientService.
    /// </summary>
    /// <param name="context">Контекст базы данных.</param>
    public IngredientService(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    #endregion

    #region Методы

    /// <summary>
    /// Получает все ингредиенты из базы данных.
    /// </summary>
    /// <returns>Список всех ингредиентов.</returns>
    public async Task<List<Ingredient>> GetAllAsync()
    {
        return await _context.Ingredients.Include(i => i.Category).ToListAsync();
    }

    /// <summary>
    /// Получает все категории из базы данных.
    /// </summary>
    /// <returns>Список всех категорий.</returns>
    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await _context.Categories.ToListAsync();
    }

    /// <summary>
    /// Добавляет новую категорию.
    /// </summary>
    /// <param name="category">Категория для добавления.</param>
    /// <returns>Добавленная категория.</returns>
    public async Task<Category> AddCategoryAsync(Category category)
    {
        if (category == null) throw new ArgumentNullException(nameof(category));
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }

    /// <summary>
    /// Добавляет новый ингредиент в базу данных.
    /// </summary>
    /// <param name="ingredient">Ингредиент для добавления.</param>
    /// <exception cref="ArgumentNullException">Если ingredient равен null.</exception>
    public async Task AddAsync(Ingredient ingredient)
    {
        if (ingredient == null)
            throw new ArgumentNullException(nameof(ingredient));

        _context.Ingredients.Add(ingredient);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Обновляет существующий ингредиент в базе данных.
    /// </summary>
    /// <param name="ingredient">Ингредиент для обновления.</param>
    /// <exception cref="ArgumentNullException">Если ingredient равен null.</exception>
    public async Task UpdateAsync(Ingredient ingredient)
    {
        if (ingredient == null)
            throw new ArgumentNullException(nameof(ingredient));

        _context.Ingredients.Update(ingredient);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Удаляет ингредиент по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор ингредиента для удаления.</param>
    /// <returns>True, если ингредиент был удален; иначе false.</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        var ingredient = await _context.Ingredients.FindAsync(id);
        if (ingredient == null)
            return false;

        _context.Ingredients.Remove(ingredient);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Поиск ингредиентов по названию.
    /// </summary>
    /// <param name="term">Текст для поиска.</param>
    /// <returns>Список найденных ингредиентов.</returns>
    public async Task<List<Ingredient>> SearchAsync(string term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return new List<Ingredient>();

        return await _context.Ingredients
            .Include(i => i.Category)
            .Where(i => i.Name.Contains(term))
            .ToListAsync();
    }

    #endregion
}

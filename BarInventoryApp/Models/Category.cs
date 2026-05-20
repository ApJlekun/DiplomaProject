namespace BarInventoryApp.Models;

/// <summary>
/// Представляет категорию ингредиента.
/// </summary>
public partial class Category
{
    /// <summary>
    /// Уникальный идентификатор категории.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Название категории.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Коллекция ингредиентов в данной категории.
    /// </summary>
    public virtual ICollection<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
}

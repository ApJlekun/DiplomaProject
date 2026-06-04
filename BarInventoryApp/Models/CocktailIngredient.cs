namespace BarInventoryApp.Models;

/// <summary>
/// Представляет связь коктейля с его ингредиентом.
/// </summary>
public partial class CocktailIngredient
{
    /// <summary>
    /// Уникальный идентификатор.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Идентификатор коктейля.
    /// </summary>
    public int CocktailId { get; set; }

    /// <summary>
    /// Идентификатор ингредиента.
    /// </summary>
    public int IngredientId { get; set; }

    /// <summary>
    /// Количество ингредиента в одном коктейле.
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// Навигационное свойство коктейля.
    /// </summary>
    public virtual Cocktail Cocktail { get; set; } = null!;

    /// <summary>
    /// Навигационное свойство ингредиента.
    /// </summary>
    public virtual Ingredient Ingredient { get; set; } = null!;
}

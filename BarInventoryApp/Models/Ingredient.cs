namespace BarInventoryApp.Models;

/// <summary>
/// Представляет ингредиент для бара (напиток, добавка и т.д.).
/// </summary>
public partial class Ingredient
{
    /// <summary>
    /// Уникальный идентификатор ингредиента.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Название ингредиента.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Количество ингредиента в наличии.
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// Единица измерения (литры, граммы, штуки и т.д.).
    /// </summary>
    public string Unit { get; set; } = null!;

    /// <summary>
    /// Идентификатор категории ингредиента.
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Навигационное свойство категории.
    /// </summary>
    public virtual Category Category { get; set; } = null!;
}

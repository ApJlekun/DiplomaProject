using System.Collections.Generic;

namespace BarInventoryApp.Models;

/// <summary>
/// Представляет коктейль (готовый напиток из ингредиентов).
/// </summary>
public partial class Cocktail
{
    /// <summary>
    /// Уникальный идентификатор коктейля.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Название коктейля.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Состав коктейля (список ингредиентов).
    /// </summary>
    public virtual ICollection<CocktailIngredient> CocktailIngredients { get; set; } = new List<CocktailIngredient>();
}

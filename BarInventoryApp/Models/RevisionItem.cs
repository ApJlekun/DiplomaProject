namespace BarInventoryApp.Models;

/// <summary>
/// Представляет позицию в ревизии.
/// </summary>
public partial class RevisionItem
{
    /// <summary>
    /// Уникальный идентификатор позиции ревизии.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Идентификатор ревизии.
    /// </summary>
    public int RevisionId { get; set; }

    /// <summary>
    /// Идентификатор ингредиента.
    /// </summary>
    public int IngredientId { get; set; }

    /// <summary>
    /// Количество по системе на момент создания/обновления.
    /// </summary>
    public decimal SystemQuantity { get; set; }

    /// <summary>
    /// Фактическое количество, введенное пользователем.
    /// </summary>
    public decimal ActualQuantity { get; set; }

    /// <summary>
    /// Разница между фактическим и системным количеством.
    /// </summary>
    public decimal Discrepancy { get; set; }

    /// <summary>
    /// Навигационное свойство ревизии.
    /// </summary>
    public virtual Revision Revision { get; set; } = null!;

    /// <summary>
    /// Навигационное свойство ингредиента.
    /// </summary>
    public virtual Ingredient Ingredient { get; set; } = null!;
}

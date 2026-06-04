namespace BarInventoryApp.Models
{
    /// <summary>
    /// Элемент чека (списываемый ингредиент или коктейль).
    /// </summary>
    public class ReceiptItem
    {
        public int Id { get; set; }
        public int ReceiptId { get; set; }
        public int? IngredientId { get; set; }
        public int? CocktailId { get; set; }
        public decimal Quantity { get; set; }

        public virtual Receipt Receipt { get; set; } = null!;
        public virtual Ingredient? Ingredient { get; set; }
        public virtual Cocktail? Cocktail { get; set; }

        public string DisplayName => Cocktail?.Name ?? Ingredient?.Name ?? "Н/Д";
        public string DisplayUnit => Cocktail != null ? "шт." : (Ingredient?.Unit ?? "Н/Д");
    }
}

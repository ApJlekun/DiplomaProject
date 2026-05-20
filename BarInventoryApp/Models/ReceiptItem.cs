namespace BarInventoryApp.Models
{
    /// <summary>
    /// Элемент чека (списываемый ингредиент).
    /// </summary>
    public class ReceiptItem
    {
        public int Id { get; set; }
        public int ReceiptId { get; set; }
        public int IngredientId { get; set; }
        public decimal Quantity { get; set; }

        public virtual Receipt Receipt { get; set; }
        public virtual Ingredient Ingredient { get; set; }
    }
}

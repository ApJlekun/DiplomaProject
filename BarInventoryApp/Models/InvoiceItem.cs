namespace BarInventoryApp.Models
{
    /// <summary>
    /// Позиция в накладной.
    /// </summary>
    public class InvoiceItem
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }
        public int IngredientId { get; set; }
        public decimal Quantity { get; set; }

        public virtual Invoice Invoice { get; set; } = null!;
        public virtual Ingredient Ingredient { get; set; } = null!;
    }
}

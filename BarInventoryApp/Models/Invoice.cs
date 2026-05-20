using System;
using System.Collections.Generic;

namespace BarInventoryApp.Models
{
    /// <summary>
    /// Модель накладной (прихода ингредиентов).
    /// </summary>
    public class Invoice
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        public string Status { get; set; } = "Не проведена";

        public virtual User CreatedByNavigation { get; set; } = null!;
        public virtual ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
    }
}

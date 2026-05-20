using System;
using System.Collections.Generic;

namespace BarInventoryApp.Models
{
    /// <summary>
    /// Модель чека (списания).
    /// </summary>
    public class Receipt
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual User User { get; set; }
        public virtual ICollection<ReceiptItem> ReceiptItems { get; set; } = new List<ReceiptItem>();
    }
}

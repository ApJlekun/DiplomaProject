using BarInventoryApp.DataContexts;
using BarInventoryApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BarInventoryApp.Services
{
    public class InvoiceService
    {
        private readonly AppDbContext _context;

        public InvoiceService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Invoice>> GetAllInvoicesAsync()
        {
            return await _context.Invoices
                .Include(i => i.CreatedByNavigation)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<InvoiceItem>> GetInvoiceItemsAsync(int invoiceId)
        {
            return await _context.InvoiceItems
                .Include(ii => ii.Ingredient)
                .Where(ii => ii.InvoiceId == invoiceId)
                .ToListAsync();
        }

        public async Task SaveInvoiceAsync(Invoice invoice, List<(int ingredientId, decimal quantity)> items, bool post)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (invoice.Id == 0)
                {
                    _context.Invoices.Add(invoice);
                }
                else
                {
                    var existing = await _context.Invoices.FindAsync(invoice.Id);
                    if (existing != null && existing.Status == "Проведена")
                        throw new Exception("Нельзя изменять проведенную накладную.");
                    
                    _context.Entry(invoice).State = EntityState.Modified;
                    
                    // Удаляем старые позиции
                    var oldItems = await _context.InvoiceItems.Where(ii => ii.InvoiceId == invoice.Id).ToListAsync();
                    _context.InvoiceItems.RemoveRange(oldItems);
                }

                await _context.SaveChangesAsync();

                foreach (var item in items)
                {
                    var invoiceItem = new InvoiceItem
                    {
                        InvoiceId = invoice.Id,
                        IngredientId = item.ingredientId,
                        Quantity = item.quantity
                    };
                    _context.InvoiceItems.Add(invoiceItem);

                    if (post)
                    {
                        var ingredient = await _context.Ingredients.FindAsync(item.ingredientId);
                        if (ingredient != null)
                        {
                            ingredient.Quantity += item.quantity;
                        }
                    }
                }

                if (post)
                {
                    invoice.Status = "Проведена";
                }
                else
                {
                    invoice.Status = "Не проведена";
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteInvoiceAsync(int id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice != null)
            {
                if (invoice.Status == "Проведена")
                    throw new Exception("Нельзя удалить проведенную накладную.");

                _context.Invoices.Remove(invoice);
                await _context.SaveChangesAsync();
            }
        }
    }
}

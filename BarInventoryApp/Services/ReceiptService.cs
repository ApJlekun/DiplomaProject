using BarInventoryApp.DataContexts;
using BarInventoryApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BarInventoryApp.Services
{
    public class ReceiptService
    {
        private readonly AppDbContext _context;

        public ReceiptService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Receipt>> GetReceiptsAsync()
        {
            return await _context.Receipts
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<ReceiptItem>> GetReceiptItemsAsync(int receiptId)
        {
            return await _context.ReceiptItems
                .Include(ri => ri.Ingredient)
                .Where(ri => ri.ReceiptId == receiptId)
                .ToListAsync();
        }

        public async Task CreateReceiptAsync(int userId, List<(int ingredientId, decimal quantity)> items)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var receipt = new Receipt
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Receipts.Add(receipt);
                await _context.SaveChangesAsync();

                foreach (var item in items)
                {
                    var ingredient = await _context.Ingredients.FindAsync(item.ingredientId);
                    if (ingredient == null)
                        throw new Exception($"Ингредиент с ID {item.ingredientId} не найден.");

                    if (ingredient.Quantity < item.quantity)
                        throw new Exception($"Недостаточно ингредиента '{ingredient.Name}' на складе. Остаток: {ingredient.Quantity}, требуется: {item.quantity}");

                    ingredient.Quantity -= item.quantity;

                    var receiptItem = new ReceiptItem
                    {
                        ReceiptId = receipt.Id,
                        IngredientId = item.ingredientId,
                        Quantity = item.quantity
                    };

                    _context.ReceiptItems.Add(receiptItem);
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
    }
}

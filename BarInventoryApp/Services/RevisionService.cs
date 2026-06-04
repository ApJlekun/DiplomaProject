using BarInventoryApp.DataContexts;
using BarInventoryApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BarInventoryApp.Services;

/// <summary>
/// Сервис для управления ревизиями (инвентаризацией).
/// </summary>
public class RevisionService
{
    private readonly AppDbContext _context;

    public RevisionService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Получает все ревизии.
    /// </summary>
    public async Task<List<Revision>> GetAllAsync()
    {
        return await _context.Revisions
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Получает ревизию по Id с её позициями.
    /// </summary>
    public async Task<Revision?> GetByIdAsync(int id)
    {
        return await _context.Revisions
            .Include(r => r.User)
            .Include(r => r.RevisionItems)
                .ThenInclude(ri => ri.Ingredient)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    /// <summary>
    /// Сохраняет или обновляет запись о ревизии без изменения остатков.
    /// </summary>
    public async Task<Revision> SaveAsync(Revision revision)
    {
        if (revision.Id == 0)
        {
            revision.CreatedAt = DateTime.UtcNow;
            revision.Status = "Сохранена";
            _context.Revisions.Add(revision);
        }
        else
        {
            var existing = await _context.Revisions.FindAsync(revision.Id);
            if (existing != null && existing.ProcessedAt.HasValue)
                throw new InvalidOperationException("Нельзя изменить уже проведенную ревизию.");
            
            _context.Entry(existing!).CurrentValues.SetValues(revision);
        }

        await _context.SaveChangesAsync();
        return revision;
    }

    /// <summary>
    /// Проводит ревизию, обновляя остатки ингредиентов в системе.
    /// </summary>
    public async Task ProcessAsync(Revision revision)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var dbRevision = await GetByIdAsync(revision.Id);
            if (dbRevision == null || dbRevision.ProcessedAt.HasValue)
                throw new InvalidOperationException("Ревизия не найдена или уже проведена.");

            foreach (var item in revision.RevisionItems)
            {
                var ingredient = await _context.Ingredients.FindAsync(item.IngredientId);
                if (ingredient != null)
                {
                    // Обновляем остатки в системе согласно фактическому количеству из ревизии
                    ingredient.Quantity = item.ActualQuantity;
                }

                // Сохраняем/обновляем позицию ревизии
                var dbItem = dbRevision.RevisionItems.FirstOrDefault(ri => ri.Id == item.Id);
                if (dbItem != null)
                {
                    dbItem.ActualQuantity = item.ActualQuantity;
                    dbItem.Discrepancy = item.ActualQuantity - dbItem.SystemQuantity;
                }
            }

            dbRevision.ProcessedAt = DateTime.UtcNow;
            dbRevision.Status = "Проведена";

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}

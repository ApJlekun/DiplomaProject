using System;
using System.Collections.Generic;

namespace BarInventoryApp.Models;

/// <summary>
/// Представляет запись о ревизии (инвентаризации).
/// </summary>
public partial class Revision
{
    /// <summary>
    /// Уникальный идентификатор ревизии.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Дата и время создания записи о ревизии.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Дата и время проведения ревизии (фиксации результатов).
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Идентификатор пользователя, создавшего запись.
    /// </summary>
    public int CreatedByUserId { get; set; }

    /// <summary>
    /// Статус ревизии (например, "Сохранена", "Проведена").
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// Навигационное свойство пользователя.
    /// </summary>
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// Коллекция позиций данной ревизии.
    /// </summary>
    public virtual ICollection<RevisionItem> RevisionItems { get; set; } = new List<RevisionItem>();
}

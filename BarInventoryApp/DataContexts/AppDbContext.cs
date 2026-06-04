using BarInventoryApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BarInventoryApp.DataContexts;

/// <summary>
/// Контекст базы данных Entity Framework для приложения управления инвентарем бара.
/// </summary>
public partial class AppDbContext : DbContext
{
    /// <summary>
    /// Конструктор по умолчанию.
    /// </summary>
    public AppDbContext()
    {
    }

    /// <summary>
    /// Конструктор с параметрами конфигурации.
    /// </summary>
    /// <param name="options">Параметры конфигурации контекста базы данных.</param>
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Набор данных ингредиентов.
    /// </summary>
    public virtual DbSet<Ingredient> Ingredients { get; set; }

    /// <summary>
    /// Набор данных категорий.
    /// </summary>
    public virtual DbSet<Category> Categories { get; set; }

    /// <summary>
    /// Набор данных накладных.
    /// </summary>
    public virtual DbSet<Invoice> Invoices { get; set; }

    /// <summary>
    /// Набор данных позиций накладных.
    /// </summary>
    public virtual DbSet<InvoiceItem> InvoiceItems { get; set; }

    /// <summary>
    /// Набор данных ролей.
    /// </summary>
    public virtual DbSet<Role> Roles { get; set; }

    /// <summary>
    /// Набор данных пользователей.
    /// </summary>
    public virtual DbSet<User> Users { get; set; }

    /// <summary>
    /// Набор данных чеков.
    /// </summary>
    public virtual DbSet<Receipt> Receipts { get; set; }

    /// <summary>
    /// Набор данных элементов чеков.
    /// </summary>
    public virtual DbSet<ReceiptItem> ReceiptItems { get; set; }

    /// <summary>
    /// Набор данных коктейлей.
    /// </summary>
    public virtual DbSet<Cocktail> Cocktails { get; set; }

    /// <summary>
    /// Набор данных ингредиентов коктейлей.
    /// </summary>
    public virtual DbSet<CocktailIngredient> CocktailIngredients { get; set; }

    /// <summary>
    /// Набор данных ревизий.
    /// </summary>
    public virtual DbSet<Revision> Revisions { get; set; }

    /// <summary>
    /// Набор данных позиций ревизий.
    /// </summary>
    public virtual DbSet<RevisionItem> RevisionItems { get; set; }

    /// <summary>
    /// Настройка подключения к базе данных.
    /// Используется только если контекст создан без параметров конфигурации.
    /// В продакшене рекомендуется всегда использовать параметры конфигурации из DI контейнера.
    /// </summary>
    /// <param name="optionsBuilder">Построитель параметров подключения.</param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Если конфигурация уже установлена через DI, не переопределяем её
        if (!optionsBuilder.IsConfigured)
        {
            // В продакшене эту строку следует удалить и всегда использовать конфигурацию из appsettings.json
            // Она оставлена только для обратной совместимости
            optionsBuilder.UseSqlServer("Data Source=DESKTOP-ETR6OD9;Initial Catalog=BarInventoryDB;User ID=ApJlukun;Password=95912060KLigNE;Encrypt=True;Trust Server Certificate=True");
        }
    }

    /// <summary>
    /// Конфигурация моделей данных при создании схемы базы данных.
    /// </summary>
    /// <param name="modelBuilder">Построитель модели данных.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Конфигурация сущности Category
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        // Конфигурация сущности Ingredient
        modelBuilder.Entity<Ingredient>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Ingredie__3214EC073873EA47");

            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Quantity).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Unit).HasMaxLength(20);

            entity.HasOne(d => d.Category).WithMany(p => p.Ingredients)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        // Конфигурация сущности Invoice
        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.CreatedByNavigation).WithMany()
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Invoices_Users");
        });

        // Конфигурация сущности InvoiceItem
        modelBuilder.Entity<InvoiceItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Quantity).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Invoice).WithMany(p => p.InvoiceItems)
                .HasForeignKey(d => d.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_InvoiceItems_Invoices");

            entity.HasOne(d => d.Ingredient).WithMany()
                .HasForeignKey(d => d.IngredientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvoiceItems_Ingredients");
        });

        // Конфигурация сущности Role
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3214EC0765CE7400");

            entity.HasIndex(e => e.Name, "UQ__Roles__737584F6412E5EB8").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        // Конфигурация сущности User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC07D5C354E3");

            entity.HasIndex(e => e.Login, "UQ__Users__5E55825B8CDD2DBE").IsUnique();

            entity.Property(e => e.Login).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Users__RoleId__3B75D760");
        });

        // Конфигурация сущности Receipt
        modelBuilder.Entity<Receipt>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.User).WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Receipts_Users");
        });

        // Конфигурация сущности ReceiptItem
        modelBuilder.Entity<ReceiptItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Quantity).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Receipt).WithMany(p => p.ReceiptItems)
                .HasForeignKey(d => d.ReceiptId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_ReceiptItems_Receipts");

            entity.HasOne(d => d.Ingredient).WithMany()
                .HasForeignKey(d => d.IngredientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReceiptItems_Ingredients");
        });

        // Конфигурация сущности Revision
        modelBuilder.Entity<Revision>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany()
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Revisions_Users");
        });

        // Конфигурация сущности RevisionItem
        modelBuilder.Entity<RevisionItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SystemQuantity).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ActualQuantity).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Discrepancy).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Revision).WithMany(p => p.RevisionItems)
                .HasForeignKey(d => d.RevisionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_RevisionItems_Revisions");

            entity.HasOne(d => d.Ingredient).WithMany()
                .HasForeignKey(d => d.IngredientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RevisionItems_Ingredients");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

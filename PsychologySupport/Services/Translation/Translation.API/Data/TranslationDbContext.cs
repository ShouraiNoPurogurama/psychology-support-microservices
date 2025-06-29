using Microsoft.EntityFrameworkCore;
using Translation.API.Models;

namespace Translation.API.Data;

public class TranslationDbContext : DbContext
{
    public TranslationDbContext(DbContextOptions<TranslationDbContext> options)
        : base(options)
    {
    }

    public DbSet<TranslatableField> TranslatableFields { get; set; } = default!;
    public DbSet<Models.Translation> Translations { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TranslatableField>(entity =>
        {
            entity.ToTable("TranslatableFields");

            entity.HasIndex(e => new { e.TableName, e.FieldName })
                .HasDatabaseName("idx_TranslatableFields_Table_Field");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<Models.Translation>(entity =>
        {
            entity.ToTable("Translations");

            entity.HasIndex(e => new { e.TextKey, e.Lang })
                .HasDatabaseName("idx_Translations_TextKey_Lang");

            entity.HasIndex(e => new { e.TextKey, e.Lang })
                .IsUnique()
                .HasDatabaseName("uq_Translations_TextKey_Lang");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });
    }
}
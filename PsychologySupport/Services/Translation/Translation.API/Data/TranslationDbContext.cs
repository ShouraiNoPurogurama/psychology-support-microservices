using Microsoft.EntityFrameworkCore;
using Translation.API.Enums;
using Translation.API.Models;

namespace Translation.API.Data;

public class TranslationDbContext : DbContext
{
    public TranslationDbContext(DbContextOptions<TranslationDbContext> options)
        : base(options)
    {
    }

    public DbSet<TranslatableField> TranslatableFields  => Set<TranslatableField>();
    public DbSet<Models.Translation> Translations => Set<Models.Translation>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema("public");

        builder.Entity<TranslatableField>(entity =>
        {
            entity.ToTable("TranslatableFields");

            entity.HasIndex(e => new { e.TableName, e.FieldName })
                .HasDatabaseName("idx_TranslatableFields_Table_Field");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });

        builder.Entity<Models.Translation>(entity =>
        {
            entity.ToTable("Translations");
            
            entity.Property(e => e.Lang)
                .HasConversion(
                    lang => lang.ToString(),                      
                    dbValue => Enum.Parse<SupportedLang>(dbValue) 
                )
                .HasMaxLength(10)
                .IsRequired();

            entity.HasIndex(e => new { e.TextKey, e.Lang })
                .HasDatabaseName("idx_Translations_TextKey_Lang");

            entity.HasIndex(e => new { e.TextKey, e.Lang })
                .IsUnique()
                .HasDatabaseName("uq_Translations_TextKey_Lang");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });
    }
}
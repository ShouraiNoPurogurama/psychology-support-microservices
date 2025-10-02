using DigitalGoods.API.Enums;
using DigitalGoods.API.Models;
using Microsoft.Build.Tasks.Deployment.Bootstrapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;

namespace DigitalGoods.API.Data;

public partial class DigitalGoodsDbContext : DbContext
{
    public DigitalGoodsDbContext()
    {
    }

    public DigitalGoodsDbContext(DbContextOptions<DigitalGoodsDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<DigitalGood> DigitalGoods { get; set; }

    public virtual DbSet<Inventory> Inventories { get; set; }

    public virtual DbSet<EmotionTag> EmotionTags { get; set; }

    public virtual DbSet<OutboxMessage> OutboxMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DigitalGood>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("digital_goods_pkey");

            entity.ToTable("digital_goods");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.ConsumptionType)
                .HasMaxLength(20)
                .HasColumnName("consumption_type");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.LastModified)
                .HasDefaultValueSql("now()")
                .HasColumnName("last_modified");
            entity.Property(e => e.LastModifiedBy).HasColumnName("last_modified_by");
            entity.Property(e => e.MediaId).HasColumnName("media_id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.Type)
                .HasMaxLength(20)
                .HasColumnName("type");
        });

        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("inventories_pkey");

            entity.ToTable("inventories");

            entity.HasIndex(e => new { e.Subject_ref, e.DigitalGoodId, e.Status }, "uq_inventory_user_good_active").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DigitalGoodId).HasColumnName("digital_good_id");
            entity.Property(e => e.ExpiredAt).HasColumnName("expired_at");
            entity.Property(e => e.GrantedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("granted_at");
            entity.Property(e => e.LastModified)
                .HasDefaultValueSql("now()")
                .HasColumnName("last_modified");
            entity.Property(e => e.LastModifiedBy).HasColumnName("last_modified_by");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");
            entity.Property(e => e.Subject_ref).HasColumnName("subject_ref");

            entity.HasOne(d => d.DigitalGood)
                .WithMany(p => p.Inventories)
                .HasForeignKey(d => d.DigitalGoodId)
                .HasConstraintName("fk_inventory_digital_good");
        });

        modelBuilder.Entity<EmotionTag>(entity =>
        {
            entity.HasIndex(e => e.Code, "unq_emotion_tags_code").IsUnique();

            entity.Property(e => e.Scope)
                       .HasConversion(new EnumToStringConverter<EmotionTagScope>())
                       .HasColumnType("VARCHAR(25)")
                       .HasColumnName("scope");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
using Billing.API.Data.Common;
using Billing.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;

namespace Billing.API.Data;

public partial class BillingDbContext : DbContext
{
    public BillingDbContext()
    {
    }

    public BillingDbContext(DbContextOptions<BillingDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<IdempotencyKey> IdempotencyKeys { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<InvoiceItem> InvoiceItems { get; set; }

    public virtual DbSet<InvoiceSnapshot> InvoiceSnapshots { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OutboxMessage> OutboxMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IdempotencyKey>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("idempotency_keys_pkey");

            entity.ToTable("idempotency_keys");

            entity.HasIndex(e => e.Key, "idempotency_keys_key_key").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");

            entity.Property(e => e.CreatedBy)
                .HasColumnName("created_by");

            entity.Property(e => e.ExpiresAt)
                .HasColumnName("expires_at");

            // Lúc này map Key -> cột mới "key"
            entity.Property(e => e.Key)
                .HasColumnName("key");

            entity.Property(e => e.RequestHash)
                .HasMaxLength(128)
                .HasColumnName("request_hash");

            entity.Property(e => e.ResponsePayload)
                .HasColumnType("jsonb")
                .HasColumnName("response_payload");
        });


        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("invoices_pkey");

            entity.ToTable("invoices");

            entity.HasIndex(e => e.Code, "invoices_code_key").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.SubjectRef).HasColumnName("subject_ref");
            entity.Property(e => e.Amount)
                .HasPrecision(15, 2)
                .HasColumnName("amount");
            entity.Property(e => e.Code)
                .HasMaxLength(20)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.IssuedAt).HasColumnName("issued_at");
            entity.Property(e => e.LastModified)
                .HasDefaultValueSql("now()")
                .HasColumnName("last_modified");
            entity.Property(e => e.LastModifiedBy).HasColumnName("last_modified_by");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.Status)
                .HasConversion(new EnumToStringConverter<InvoiceStatus>())
                .HasColumnType("VARCHAR(20)")
                .HasColumnName("status");

            entity.HasOne(d => d.Order).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("invoices_order_id_fkey");
        });

        modelBuilder.Entity<InvoiceItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("invoice_items_pkey");

            entity.ToTable("invoice_items");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DiscountAmount)
                .HasPrecision(15, 2)
                .HasColumnName("discount_amount");
            entity.Property(e => e.InvoiceSnapshotId).HasColumnName("invoice_snapshot_id");
            entity.Property(e => e.ItemType)
                .HasMaxLength(20)
                .HasColumnName("item_type");
            entity.Property(e => e.LastModified)
                .HasDefaultValueSql("now()")
                .HasColumnName("last_modified");
            entity.Property(e => e.LastModifiedBy).HasColumnName("last_modified_by");
            entity.Property(e => e.ProductCode)
                .HasMaxLength(50)
                .HasColumnName("product_code");
            entity.Property(e => e.ProductName).HasColumnName("product_name");
            entity.Property(e => e.PromoCode)
                .HasMaxLength(50)
                .HasColumnName("promo_code");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(15, 2)
                .HasColumnName("total_amount");
            entity.Property(e => e.Unit)
                .HasMaxLength(20)
                .HasColumnName("unit");
            entity.Property(e => e.UnitPrice)
                .HasPrecision(15, 2)
                .HasColumnName("unit_price");

            entity.HasOne(d => d.InvoiceSnapshot).WithMany(p => p.InvoiceItems)
                .HasForeignKey(d => d.InvoiceSnapshotId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("invoice_items_invoice_snapshot_id_fkey");
        });

        modelBuilder.Entity<InvoiceSnapshot>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("invoice_snapshots_pkey");

            entity.ToTable("invoice_snapshots");

            entity.HasIndex(e => e.InvoiceId, "invoice_snapshots_invoice_id_key").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.AliasInfo)
                .HasColumnType("jsonb")
                .HasColumnName("alias_info");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .HasColumnName("currency");
            entity.Property(e => e.InvoiceId).HasColumnName("invoice_id");
            entity.Property(e => e.LastModified)
                .HasDefaultValueSql("now()")
                .HasColumnName("last_modified");
            entity.Property(e => e.LastModifiedBy).HasColumnName("last_modified_by");
            entity.Property(e => e.OrderType)
                .HasMaxLength(20)
                .HasColumnName("order_type");
            entity.Property(e => e.TaxAmount)
                .HasPrecision(15, 2)
                .HasColumnName("tax_amount");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(15, 2)
                .HasColumnName("total_amount");
            entity.Property(e => e.TotalDiscountAmount)
                .HasPrecision(15, 2)
                .HasColumnName("total_discount_amount");

            entity.HasOne(d => d.Invoice).WithOne(p => p.InvoiceSnapshot)
                .HasForeignKey<InvoiceSnapshot>(d => d.InvoiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("invoice_snapshots_invoice_id_fkey");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("orders_pkey");

            entity.ToTable("orders");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.SubjectRef).HasColumnName("subject_ref");
            entity.Property(e => e.Amount)
                .HasPrecision(15, 2)
                .HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .HasColumnName("currency");

            entity.Property(e => e.ProductCode)
                .HasMaxLength(50)
                .HasColumnName("product_code");

            entity.Property(e => e.IdempotencyKeyId).HasColumnName("idempotency_key_id");
            entity.Property(e => e.InvoiceId).HasColumnName("invoice_id");
            entity.Property(e => e.LastModified)
                .HasDefaultValueSql("now()")
                .HasColumnName("last_modified");
            entity.Property(e => e.LastModifiedBy).HasColumnName("last_modified_by");
            entity.Property(e => e.OrderType)
                .HasMaxLength(20)
                .HasColumnName("order_type");
            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.PromoCode)
                .HasMaxLength(50)
                .HasColumnName("promo_code");
            entity.Property(e => e.Status)
                .HasConversion(new EnumToStringConverter<OrderStatus>())
                .HasColumnType("VARCHAR(20)")
                .HasColumnName("status");

            entity.HasOne(d => d.IdempotencyKey).WithMany(p => p.Orders)
                .HasForeignKey(d => d.IdempotencyKeyId)
                .HasConstraintName("orders_idempotency_key_id_fkey");
        });


        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("outbox_messages_pkey");

            entity.ToTable("outbox_messages");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.AggregateId).HasColumnName("aggregate_id");
            entity.Property(e => e.AggregateType)
                .HasMaxLength(50)
                .HasColumnName("aggregate_type");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.EventType)
                .HasMaxLength(50)
                .HasColumnName("event_type");
            entity.Property(e => e.LastModified)
                .HasDefaultValueSql("now()")
                .HasColumnName("last_modified");
            entity.Property(e => e.LastModifiedBy).HasColumnName("last_modified_by");
            entity.Property(e => e.OccurredOn)
                .HasDefaultValueSql("now()")
                .HasColumnName("occurred_on");
            entity.Property(e => e.Payload)
                .HasColumnType("jsonb")
                .HasColumnName("payload");
            entity.Property(e => e.ProcessedOn).HasColumnName("processed_on");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

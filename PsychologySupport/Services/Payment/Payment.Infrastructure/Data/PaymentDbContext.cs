using Microsoft.EntityFrameworkCore;
using Payment.Application.Data;
using Payment.Domain.Enums;
using Payment.Domain.Models;

namespace Payment.Infrastructure.Data;

public class PaymentDbContext : DbContext, IPaymentDbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options)
    {
    }

    public DbSet<Domain.Models.Payment> Payments => Set<Domain.Models.Payment>();
    public DbSet<PaymentDetail> PaymentDetails => Set<PaymentDetail>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Domain.Models.Payment>(e =>
        {
            e.Property(p => p.Status)
                .HasDefaultValue(PaymentStatus.Pending)
                .HasConversion(s => s.ToString(),
                    dbStatus => (PaymentStatus)Enum.Parse(typeof(PaymentStatus), dbStatus));

            e.Property(p => p.TotalAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();
        });

        builder.Entity<PaymentDetail>(e =>
        {
            e.Property(p => p.Status)
                .HasDefaultValue(PaymentDetailStatus.Pending)
                .HasConversion(s => s.ToString(),
                    dbStatus => (PaymentDetailStatus)Enum.Parse(typeof(PaymentDetailStatus), dbStatus));

            e.Property(p => p.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();
        });
    }
}
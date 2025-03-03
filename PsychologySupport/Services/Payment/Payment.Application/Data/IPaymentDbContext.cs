using Microsoft.EntityFrameworkCore;

namespace Payment.Application.Data;

public interface IPaymentDbContext
{
    DbSet<Domain.Models.Payment> Payments { get; }
    DbSet<PaymentDetail> PaymentDetails { get; }
    DbSet<PaymentMethod> PaymentMethods { get; }


    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
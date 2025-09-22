using Billing.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Billing.Application.Data
{
    public interface IBillingDbContext
    {
        DbSet<Invoice> Invoices { get; set; }

        DbSet<InvoiceItem> InvoiceItems { get; set; }

        DbSet<InvoiceSnapshot> InvoiceSnapshots { get; set; }

        DbSet<Order> Orders { get; set; }

        DbSet<OutboxMessage> OutboxMessages { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);

        Task<IDbContextTransaction> BeginTransactionAsync(
            IsolationLevel isolationLevel,
            CancellationToken cancellationToken = default);
    }
}

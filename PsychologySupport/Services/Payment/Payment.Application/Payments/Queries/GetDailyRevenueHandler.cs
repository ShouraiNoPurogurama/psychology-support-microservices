using MediatR;
using Microsoft.EntityFrameworkCore;
using Payment.Application.Data;
using Payment.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payment.Application.Payments.Queries
{
    public record GetDailyRevenueQuery(DateOnly StartTime, DateOnly EndTime) : IRequest<GetDailyRevenueResult>;

    public record DailyRevenue(DateOnly Date, decimal TotalRevenue, float TotalPayment);

    public record GetDailyRevenueResult(List<DailyRevenue> Revenues);

    public class GetDailyRevenueHandler(IPaymentDbContext dbContext)
        : IRequestHandler<GetDailyRevenueQuery, GetDailyRevenueResult>
    {
        public async Task<GetDailyRevenueResult> Handle(GetDailyRevenueQuery request, CancellationToken cancellationToken)
        {
            var payments = await dbContext.Payments
                .Where(p => p.Status == PaymentStatus.Completed &&
                            p.CreatedAt.HasValue &&
                            DateOnly.FromDateTime(p.CreatedAt.Value.UtcDateTime) >= request.StartTime &&
                            DateOnly.FromDateTime(p.CreatedAt.Value.UtcDateTime) <= request.EndTime)
                .ToListAsync(cancellationToken); 

            var revenueByDate = payments
                .GroupBy(p => DateOnly.FromDateTime(p.CreatedAt.Value.UtcDateTime))
                .Select(g => new DailyRevenue(g.Key, g.Sum(p => p.TotalAmount), g.Count()))
                .ToList();

            return new GetDailyRevenueResult(revenueByDate);
        }
    }
}

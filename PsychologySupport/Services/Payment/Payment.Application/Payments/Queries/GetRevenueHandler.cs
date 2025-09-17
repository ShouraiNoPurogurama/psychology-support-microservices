using MediatR;
using Microsoft.EntityFrameworkCore;
using Payment.Application.Data;
using Payment.Domain.Enums;

namespace Payment.Application.Payments.Queries
{
    public record GetRevenueQuery(DateOnly StartTime, DateOnly EndTime) : IRequest<GetRevenueResult>;

    public record GetRevenueResult(decimal TotalRevenue);

    internal class GetRevenueHandler(IPaymentDbContext dbContext)
        : IRequestHandler<GetRevenueQuery, GetRevenueResult>
    {
        public async Task<GetRevenueResult> Handle(GetRevenueQuery request, CancellationToken cancellationToken)
        {
            var totalRevenue = await dbContext.Payments
                .Where(p => p.Status == PaymentStatus.Completed &&
                            DateOnly.FromDateTime(p.CreatedAt.UtcDateTime) >= request.StartTime &&
                            DateOnly.FromDateTime(p.CreatedAt.UtcDateTime) <= request.EndTime)
                .SumAsync(p => p.TotalAmount, cancellationToken);

            return new GetRevenueResult(totalRevenue);
        }
    }
}

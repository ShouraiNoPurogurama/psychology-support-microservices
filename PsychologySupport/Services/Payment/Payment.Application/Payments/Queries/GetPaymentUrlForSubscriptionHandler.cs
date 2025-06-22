using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using Payment.Application.Data;
using Payment.Domain.Enums;

namespace Payment.Application.Payments.Queries;

public record GetPaymentUrlForSubscriptionQuery(Guid SubscriptionId) : IQuery<GetPaymentUrlForSubscriptionResult>;

public record GetPaymentUrlForSubscriptionResult(long? PaymentCode,string? Url);

public class GetPaymentUrlForSubscriptionHandler(IPaymentDbContext dbContext) : IQueryHandler<GetPaymentUrlForSubscriptionQuery, GetPaymentUrlForSubscriptionResult>
{
    public async Task<GetPaymentUrlForSubscriptionResult> Handle(GetPaymentUrlForSubscriptionQuery request, CancellationToken cancellationToken)
    {
        var payment = await dbContext.Payments
            .FirstOrDefaultAsync(p => p.SubscriptionId == request.SubscriptionId && p.Status == PaymentStatus.Pending, cancellationToken: cancellationToken);
        
        return new GetPaymentUrlForSubscriptionResult(payment?.PaymentCode,payment?.PaymentUrl);
    }
}
using BuildingBlocks.CQRS;
using Subscription.API.Data;
using Subscription.API.Exceptions;
using Subscription.API.UserSubscriptions.Dtos;

namespace Subscription.API.UserSubscriptions.Features.GetUserSubscription;

public record GetUserSubscriptionQuery(Guid Id) : IQuery<GetUserSubscriptionResult>;

public record GetUserSubscriptionResult(GetUserSubscriptionDto UserSubscription);
public class GetUserSubscriptionHandler : IQueryHandler<GetUserSubscriptionQuery, GetUserSubscriptionResult>
{
    private readonly SubscriptionDbContext _context;

    public GetUserSubscriptionHandler(SubscriptionDbContext context)
    {
        _context = context;
    }

    public async Task<GetUserSubscriptionResult> Handle(GetUserSubscriptionQuery query, CancellationToken cancellationToken)
    {
        var userSubscription = await _context.UserSubscriptions.FindAsync(query.Id, cancellationToken)
                               ?? throw new SubscriptionNotFoundException(query.Id.ToString());

        var userSubscriptionDto = new GetUserSubscriptionDto(
            userSubscription.Id,
            userSubscription.PatientId,
            userSubscription.ServicePackageId,
            userSubscription.StartDate,
            userSubscription.EndDate,
            userSubscription.PromoCodeId,
            userSubscription.GiftId,
            userSubscription.Status
        );

        return new GetUserSubscriptionResult(userSubscriptionDto);
    }
}
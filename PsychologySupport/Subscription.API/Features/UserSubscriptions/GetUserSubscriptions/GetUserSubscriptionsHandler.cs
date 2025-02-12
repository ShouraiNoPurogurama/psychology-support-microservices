using BuildingBlocks.CQRS;
using Subscription.API.Data;
using Subscription.API.Models;
using Microsoft.EntityFrameworkCore;


namespace Subscription.API.Features.UserSubscriptions.GetUserSubscriptions;

public record GetUserSubscriptionsQuery(int PageNumber, int PageSize) : IQuery<GetUserSubscriptionsResult>;

public record GetUserSubscriptionsResult(IEnumerable<UserSubscription> UserSubscriptions, int TotalCount);

public class GetUserSubscriptionsHandler : IQueryHandler<GetUserSubscriptionsQuery, GetUserSubscriptionsResult>
{
    private readonly SubscriptionDbContext _context;

    public GetUserSubscriptionsHandler(SubscriptionDbContext context)
    {
        _context = context;
    }

    public async Task<GetUserSubscriptionsResult> Handle(GetUserSubscriptionsQuery request, CancellationToken cancellationToken)
    {
        var skip = (request.PageNumber - 1) * request.PageSize;
        var totalCount = await _context.UserSubscriptions.CountAsync(cancellationToken);
        var subscriptions = await _context.UserSubscriptions
            .Skip(skip)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new GetUserSubscriptionsResult(subscriptions, totalCount);
    }
}
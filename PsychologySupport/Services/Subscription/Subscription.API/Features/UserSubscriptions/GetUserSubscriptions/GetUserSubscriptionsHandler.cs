using BuildingBlocks.CQRS;
using Subscription.API.Data;
using Subscription.API.Models;
using Microsoft.EntityFrameworkCore;
using Subscription.API.Dtos;
using BuildingBlocks.Pagination;

namespace Subscription.API.Features.UserSubscriptions.GetUserSubscriptions;

public record GetUserSubscriptionsQuery(PaginationRequest PaginationRequest) : IQuery<GetUserSubscriptionsResult>;

public record GetUserSubscriptionsResult(IEnumerable<GetUserSubscriptionDto> UserSubscriptions, int TotalCount);

public class GetUserSubscriptionsHandler : IQueryHandler<GetUserSubscriptionsQuery, GetUserSubscriptionsResult>
{
    private readonly SubscriptionDbContext _context;

    public GetUserSubscriptionsHandler(SubscriptionDbContext context)
    {
        _context = context;
    }

    public async Task<GetUserSubscriptionsResult> Handle(GetUserSubscriptionsQuery request, CancellationToken cancellationToken)
    {
        var skip = (request.PaginationRequest.PageIndex - 1) * request.PaginationRequest.PageSize;

        var totalCount = await _context.UserSubscriptions.CountAsync(cancellationToken);

        var subscriptions = await _context.UserSubscriptions
            .Skip(skip)
            .Take(request.PaginationRequest.PageSize)
            .Select(us => new GetUserSubscriptionDto(
                us.Id,
                us.PatientId,
                us.ServicePackageId,
                us.StartDate,
                us.EndDate,
                us.PromoCodeId,
                us.GiftId,
                us.Status.ToString()
            ))
            .ToListAsync(cancellationToken);

        return new GetUserSubscriptionsResult(subscriptions, totalCount);
    }
}

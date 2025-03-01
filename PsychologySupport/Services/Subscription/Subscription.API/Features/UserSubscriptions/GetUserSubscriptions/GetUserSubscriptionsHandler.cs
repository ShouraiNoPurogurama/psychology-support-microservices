using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Microsoft.EntityFrameworkCore;
using Subscription.API.Data;
using Subscription.API.Dtos;

namespace Subscription.API.Features.UserSubscriptions.GetUserSubscriptions;

public record GetUserSubscriptionsQuery(PaginationRequest PaginationRequest) : IQuery<GetUserSubscriptionsResult>;

public record GetUserSubscriptionsResult(PaginatedResult<GetUserSubscriptionDto> UserSubscriptions);

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

        var totalCount = await _context.UserSubscriptions.LongCountAsync(cancellationToken);

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
                us.Status
            ))
            .ToListAsync(cancellationToken);

        return new GetUserSubscriptionsResult(new PaginatedResult<GetUserSubscriptionDto>(request.PaginationRequest.PageIndex,
            request.PaginationRequest.PageSize, totalCount, subscriptions));
    }
}
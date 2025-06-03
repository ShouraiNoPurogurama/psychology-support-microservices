using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Subscription.API.Data;
using Subscription.API.Data.Common;
using Subscription.API.UserSubscriptions.Dtos;

namespace Subscription.API.UserSubscriptions.Features.GetUserSubscriptions;

public record GetUserSubscriptionsQuery(
        [FromQuery] int PageIndex = 1,
        [FromQuery] int PageSize = 10,
        [FromQuery] string? Search = "", // ServicePackageId
        [FromQuery] string? SortBy = "StartDate", // sort StartDate
        [FromQuery] string? SortOrder = "asc", // asc or desc
        [FromQuery] Guid? ServicePackageId = null, // filter
        [FromQuery] Guid? PatientId = null, // filter
        [FromQuery] SubscriptionStatus? Status = null) : IQuery<GetUserSubscriptionsResult>;

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
        var pageIndex = Math.Max(0, request.PageIndex - 1);
        var pageSize = request.PageSize;

        var query = _context.UserSubscriptions.AsQueryable();

        // Filtering
        if (request.ServicePackageId.HasValue)
            query = query.Where(us => us.ServicePackageId == request.ServicePackageId);

        if (request.PatientId.HasValue)
            query = query.Where(us => us.PatientId == request.PatientId);

        if (request.Status.HasValue)
            query = query.Where(us => us.Status == request.Status);

        // Search
        if (!string.IsNullOrEmpty(request.Search))
        {
            query = query.Where(us => us.ServicePackageId.ToString() == request.Search);
        }

        // Sorting
        if (request.SortBy == "StartDate")
        {
            query = request.SortOrder == "asc"
                ? query.OrderBy(us => us.StartDate)
                : query.OrderByDescending(us => us.StartDate);
        }

        // Pagination
        var totalCount = await query.CountAsync(cancellationToken);

        var subscriptions = await query
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
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

        return new GetUserSubscriptionsResult(new PaginatedResult<GetUserSubscriptionDto>(
            pageIndex + 1,
            pageSize,
            totalCount,
            subscriptions
        ));
    }
}
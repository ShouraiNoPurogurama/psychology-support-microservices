using BuildingBlocks.CQRS;
using Subscription.API.Data.Common;
using Subscription.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Subscription.API.UserSubscriptions.Features02.GetTotalSubscription
{
    public record GetTotalSubscriptionQuery(
        DateOnly StartDate,
        DateOnly EndDate,
        Guid? PatientId = null,
        SubscriptionStatus? Status = null) : IQuery<GetTotalSubscriptionResult>;

    public record GetTotalSubscriptionResult(long TotalCount);

    public class GetTotalSubscriptionHandler : IQueryHandler<GetTotalSubscriptionQuery, GetTotalSubscriptionResult>
    {
        private readonly SubscriptionDbContext _context;

        public GetTotalSubscriptionHandler(SubscriptionDbContext context)
        {
            _context = context;
        }

        public async Task<GetTotalSubscriptionResult> Handle(GetTotalSubscriptionQuery request, CancellationToken cancellationToken)
        {
            var query = _context.UserSubscriptions.AsQueryable();

            // Filter by date range using CreatedAt
            query = query.Where(us =>
                us.CreatedAt.HasValue &&
                DateOnly.FromDateTime(us.CreatedAt.Value.UtcDateTime) >= request.StartDate &&
                DateOnly.FromDateTime(us.CreatedAt.Value.UtcDateTime) <= request.EndDate);

            // Filter by PatientId if provided
            if (request.PatientId.HasValue)
            {
                query = query.Where(us => us.PatientId == request.PatientId);
            }

            // Filter by Status if provided
            if (request.Status.HasValue)
            {
                query = query.Where(us => us.Status == request.Status);
            }

            var totalCount = await query.LongCountAsync(cancellationToken);

            return new GetTotalSubscriptionResult(totalCount);
        }
    }
}

using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using Subscription.API.Data;
using Subscription.API.Data.Common;

namespace Subscription.API.UserSubscriptions.Features.v1.GetTotalServicePackage
{

    public record GetTotalServicePackageQuery(DateOnly StartDate, DateOnly EndDate) : IQuery<List<ServicePackageWithTotal>>;

    public record ServicePackageWithTotal(Guid Id, string Name, long TotalSubscriptions);

    public class GetTotalServicePackageHandler : IQueryHandler<GetTotalServicePackageQuery, List<ServicePackageWithTotal>>
    {
        private readonly SubscriptionDbContext _context;

        public GetTotalServicePackageHandler(SubscriptionDbContext context)
        {
            _context = context;
        }

        public async Task<List<ServicePackageWithTotal>> Handle(GetTotalServicePackageQuery request, CancellationToken cancellationToken)
        {
            var servicePackages = await _context.ServicePackages
                .Where(sp => sp.IsActive)
                .Select(sp => new { sp.Id, sp.Name })
                .ToListAsync(cancellationToken);

            var subscriptionCounts = await _context.UserSubscriptions
                .Where(us =>
                    us.Status == SubscriptionStatus.Active &&
                    us.CreatedAt.HasValue &&
                    DateOnly.FromDateTime(us.CreatedAt.Value.UtcDateTime) >= request.StartDate &&
                    DateOnly.FromDateTime(us.CreatedAt.Value.UtcDateTime) <= request.EndDate)
                .GroupBy(us => us.ServicePackageId)
                .Select(g => new { ServicePackageId = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            var result = servicePackages
                .Select(sp => new ServicePackageWithTotal(
                    sp.Id,
                    sp.Name,
                    subscriptionCounts.FirstOrDefault(sc => sc.ServicePackageId == sp.Id)?.Count ?? 0
                ))
                .OrderByDescending(sp => sp.TotalSubscriptions) 
                .ToList();

            return result;
        }
    }
}

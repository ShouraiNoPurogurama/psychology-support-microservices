using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using Subscription.API.Data;
using Subscription.API.Models;

namespace Subscription.API.Features.ServicePackages.GetServicePackages;

public record GetServicePackagesQuery(int PageNumber, int PageSize) : IQuery<GetServicePackagesResult>;

public record GetServicePackagesResult(IEnumerable<ServicePackage> ServicePackages, int TotalCount);

public class GetServicePackagesHandler : IQueryHandler<GetServicePackagesQuery, GetServicePackagesResult>
{
    private readonly SubscriptionDbContext _dbContext;

    public GetServicePackagesHandler(SubscriptionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetServicePackagesResult> Handle(GetServicePackagesQuery request, CancellationToken cancellationToken)
    {
        var skip = (request.PageNumber - 1) * request.PageSize;

        var totalCount = await _dbContext.ServicePackages.CountAsync(cancellationToken);

        var servicePackages = await _dbContext.ServicePackages
            .Skip(skip)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new GetServicePackagesResult(servicePackages, totalCount);
    }
}

using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using Subscription.API.Data;
using Subscription.API.Dtos;

namespace Subscription.API.Features.ServicePackages.GetServicePackages;

public record GetServicePackagesQuery(int PageNumber, int PageSize) : IQuery<GetServicePackagesResult>;

public record GetServicePackagesResult(IEnumerable<ServicePackageDto> ServicePackages, int TotalCount);

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
            .Select(sp => new ServicePackageDto(
                sp.Id,
                sp.Name,
                sp.Description,
                sp.Price,
                sp.DurationDays,
                sp.ImageId,
                sp.IsActive
            ))
            .ToListAsync(cancellationToken);

        return new GetServicePackagesResult(servicePackages, totalCount);
    }

}

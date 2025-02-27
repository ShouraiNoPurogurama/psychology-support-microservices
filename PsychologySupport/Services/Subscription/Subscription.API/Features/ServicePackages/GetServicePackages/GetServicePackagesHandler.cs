using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Microsoft.EntityFrameworkCore;
using Subscription.API.Data;
using Subscription.API.Dtos;

namespace Subscription.API.Features.ServicePackages.GetServicePackages;

public record GetServicePackagesQuery(PaginationRequest PaginationRequest) : IQuery<GetServicePackagesResult>;

public record GetServicePackagesResult(PaginatedResult<ServicePackageDto> ServicePackages);

public class GetServicePackagesHandler : IQueryHandler<GetServicePackagesQuery, GetServicePackagesResult>
{
    private readonly SubscriptionDbContext _dbContext;

    public GetServicePackagesHandler(SubscriptionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetServicePackagesResult> Handle(GetServicePackagesQuery request, CancellationToken cancellationToken)
    {
        var pageSize= request.PaginationRequest.PageSize;
        var pageIndex= request.PaginationRequest.PageIndex;
        var skip = (pageIndex - 1) * pageSize;

        var totalCount = await _dbContext.ServicePackages.LongCountAsync(cancellationToken);

        var servicePackages = await _dbContext.ServicePackages
            .OrderByDescending(sp => sp.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
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

        return new GetServicePackagesResult(new PaginatedResult<ServicePackageDto>(pageIndex, pageSize, totalCount, servicePackages));
    }

}

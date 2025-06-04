using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Microsoft.EntityFrameworkCore;
using Subscription.API.Data;
using Subscription.API.Data.Common;
using Subscription.API.ServicePackages.Dtos;
using Subscription.API.ServicePackages.Enums;

namespace Subscription.API.ServicePackages.Features.GetServicePackages;

public record GetServicePackagesQuery(
    int PageIndex = 1,
    int PageSize = 10,
    string? Search = "", // Name vs Description
    bool? Status = null, // filter
    Guid? PatientId = null // filter
) : IQuery<GetServicePackagesResult>;

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
        var pageSize = request.PageSize;
        var pageIndex = request.PageIndex;
        var search = request.Search?.Trim().ToLower();

        var query = _dbContext.ServicePackages.AsQueryable();

        if (request.Status.HasValue)
        {
            query = query.Where(sp => sp.IsActive == request.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(sp => sp.Name.ToLower().Contains(search) || sp.Description.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var servicePackages = await query
            .OrderByDescending(sp => sp.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
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

        if (request.PatientId is not null)
        {
            var patientSubscriptions = await _dbContext.UserSubscriptions
                .Where(u => u.PatientId == request.PatientId && u.EndDate >= DateTime.UtcNow &&
                    u.Status == SubscriptionStatus.Active || u.Status == SubscriptionStatus.AwaitPayment)
                .ToDictionaryAsync(u => u.ServicePackageId, u =>
                {
                    return u.Status switch
                    {
                        SubscriptionStatus.Active => ServicePackageBuyStatus.Purchased,
                        SubscriptionStatus.AwaitPayment => ServicePackageBuyStatus.PendingPayment,
                        _ => ServicePackageBuyStatus.NotPurchased
                    };
                }, cancellationToken: cancellationToken);

            foreach (var servicePackage in servicePackages)
            {
                servicePackage.PurchaseStatus = patientSubscriptions.TryGetValue(servicePackage.Id, out var status)
                    ? status.ToString(): nameof(ServicePackageBuyStatus.NotPurchased);
            }
        }

        var result = new PaginatedResult<ServicePackageDto>(pageIndex, pageSize, totalCount, servicePackages);
        return new GetServicePackagesResult(result);
    }
}
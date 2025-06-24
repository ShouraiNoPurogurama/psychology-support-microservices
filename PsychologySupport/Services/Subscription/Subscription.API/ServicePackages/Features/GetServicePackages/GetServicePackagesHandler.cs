using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Microsoft.EntityFrameworkCore;
using Subscription.API.Data;
using Subscription.API.Data.Common;
using Subscription.API.ServicePackages.Dtos;
using Subscription.API.ServicePackages.Enums;
using Subscription.API.UserSubscriptions.Models;

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

        Dictionary<Guid, ServicePackageBuyStatus> patientSubscriptions = new();
        UserSubscription? currentSubscription = null;
        decimal? currentPriceLeft = null;

        if (request.PatientId is not null)
        {
            // Get current active subscription
            currentSubscription = await _dbContext.UserSubscriptions
                .Include(us => us.ServicePackage)
                .Where(us => us.PatientId == request.PatientId
                    && us.Status == SubscriptionStatus.Active
                    && us.EndDate >= DateTime.UtcNow)
                .OrderByDescending(us => us.StartDate)
                .FirstOrDefaultAsync(cancellationToken);

            if (currentSubscription != null)
            {
                var remainingDays = (int)Math.Floor((currentSubscription.EndDate - DateTime.UtcNow).TotalDays);
                var duration = currentSubscription.ServicePackage.DurationDays;

                if (duration > 0 && remainingDays > 0)
                {
                    currentPriceLeft = Math.Round(
                        currentSubscription.FinalPrice * ((decimal)remainingDays / duration),
                        2,
                        MidpointRounding.AwayFromZero
                    );
                }
            }

            // Get subscriptions with status Active or AwaitPayment
            patientSubscriptions = await _dbContext.UserSubscriptions
                .Where(u => u.PatientId == request.PatientId
                            && u.EndDate >= DateTime.UtcNow
                            && (u.Status == SubscriptionStatus.Active || u.Status == SubscriptionStatus.AwaitPayment))
                .ToDictionaryAsync(
                    u => u.ServicePackageId,
                    u => u.Status == SubscriptionStatus.Active
                        ? ServicePackageBuyStatus.Purchased
                        : ServicePackageBuyStatus.PendingPayment,
                    cancellationToken);
        }

        // Update each servicePackage with UpgradePrice and PurchaseStatus
        foreach (var servicePackage in servicePackages)
        {
            if (currentSubscription != null)
            {
                var upgradePrice = servicePackage.Price - (currentPriceLeft ?? 0);

                if (upgradePrice > 0 && servicePackage.Id != currentSubscription.ServicePackageId)
                {
                    servicePackage.UpgradePrice = upgradePrice;
                }
            }

            servicePackage.PurchaseStatus = patientSubscriptions.TryGetValue(servicePackage.Id, out var status)
                ? status.ToString()
                : nameof(ServicePackageBuyStatus.NotPurchased);
        }

        var result = new PaginatedResult<ServicePackageDto>(pageIndex, pageSize, totalCount, servicePackages);
        return new GetServicePackagesResult(result);
    }
}

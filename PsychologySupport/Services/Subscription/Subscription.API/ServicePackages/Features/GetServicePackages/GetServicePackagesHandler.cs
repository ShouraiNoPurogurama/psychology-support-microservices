using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using BuildingBlocks.Messaging.Events.Translation;
using BuildingBlocks.Pagination;
using BuildingBlocks.Utils;
using Microsoft.EntityFrameworkCore;
using Subscription.API.Data;
using Subscription.API.ServicePackages.Dtos;
using Subscription.API.ServicePackages.Enums;
using Subscription.API.UserSubscriptions.Models;
using MassTransit;
using Subscription.API.Data.Common;
using Subscription.API.ServicePackages.Models;

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
    private readonly IRequestClient<GetTranslatedDataRequest> _translationClient;

    public GetServicePackagesHandler(
        SubscriptionDbContext dbContext,
        IRequestClient<GetTranslatedDataRequest> translationClient)
    {
        _dbContext = dbContext;
        _translationClient = translationClient;
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
            query = query.Where(sp =>
                sp.Name.ToLower().Contains(search) ||
                sp.Description.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var rawPackages = await query
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

        //1. Dịch Name & Description
        var translationDict = TranslationUtils.CreateBuilder()
            .AddEntities(rawPackages, nameof(ServicePackage), x => x.Name, x => x.Description)
            .Build();

        var response = await _translationClient.GetResponse<GetTranslatedDataResponse>(
            new GetTranslatedDataRequest(translationDict, SupportedLang.vi),
            cancellationToken);

        var translations = response.Message.Translations;

        //2. Update dữ liệu sau dịch + PurchaseStatus & UpgradePrice
        var finalPackages = rawPackages.Select(sp =>
        {
            var translatedName = translations.GetTranslatedValue(sp, x => x.Name, nameof(ServicePackage));
            var translatedDesc = translations.GetTranslatedValue(sp, x => x.Description, nameof(ServicePackage));

            if (currentSubscription != null && sp.Id != currentSubscription.ServicePackageId)
            {
                var upgradePrice = sp.Price - (currentPriceLeft ?? 0);
                if (upgradePrice > 0)
                {
                    sp.UpgradePrice = upgradePrice;
                }
            }

            sp.PurchaseStatus = patientSubscriptions.TryGetValue(sp.Id, out var status)
                ? status.ToString()
                : nameof(ServicePackageBuyStatus.NotPurchased);

            return sp with
            {
                Name = translatedName,
                Description = translatedDesc
            };
        }).ToList();

        var result = new PaginatedResult<ServicePackageDto>(pageIndex, pageSize, totalCount, finalPackages);
        return new GetServicePackagesResult(result);
    }
}

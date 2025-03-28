using BuildingBlocks.CQRS;
using Subscription.API.Data;
using Subscription.API.Exceptions;
using Subscription.API.ServicePackages.Dtos;

namespace Subscription.API.ServicePackages.Features.GetServicePackage;

public record GetServicePackageQuery(Guid Id) : IQuery<GetServicePackageResult>;

public record GetServicePackageResult(ServicePackageDto ServicePackage);

public class GetServicePackageHandler : IQueryHandler<GetServicePackageQuery, GetServicePackageResult>
{
    private readonly SubscriptionDbContext _context;

    public GetServicePackageHandler(SubscriptionDbContext context)
    {
        _context = context;
    }

    public async Task<GetServicePackageResult> Handle(GetServicePackageQuery query, CancellationToken cancellationToken)
    {
        var servicePackage = await _context.ServicePackages.FindAsync(query.Id, cancellationToken)
                             ?? throw new SubscriptionNotFoundException("Service Package", query.Id);

        return new GetServicePackageResult(new ServicePackageDto(
            servicePackage.Id,
            servicePackage.Name,
            servicePackage.Description,
            servicePackage.Price,
            servicePackage.DurationDays,
            servicePackage.ImageId,
            servicePackage.IsActive
        ));
    }
}
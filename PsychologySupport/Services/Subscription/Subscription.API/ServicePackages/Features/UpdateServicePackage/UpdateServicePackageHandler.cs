using BuildingBlocks.CQRS;
using Mapster;
using Subscription.API.Data;
using Subscription.API.Exceptions;
using Subscription.API.ServicePackages.Dtos;

namespace Subscription.API.ServicePackages.Features.UpdateServicePackage;

public record UpdateServicePackageCommand(Guid Id, UpdateServicePackageDto ServicePackage) : ICommand<UpdateServicePackageResult>;

public record UpdateServicePackageResult(bool IsSuccess);

public class UpdateServicePackageHandler(SubscriptionDbContext context)
    : ICommandHandler<UpdateServicePackageCommand, UpdateServicePackageResult>
{
    public async Task<UpdateServicePackageResult> Handle(UpdateServicePackageCommand request, CancellationToken cancellationToken)
    {
        var existingPackage = await context.ServicePackages.FindAsync([request.Id], cancellationToken)
                              ?? throw new SubscriptionNotFoundException("Service Package", request.Id);

        existingPackage.Update(
            request.ServicePackage.Name,
            request.ServicePackage.Description,
            request.ServicePackage.Price,
            request.ServicePackage.DurationDays,
            request.ServicePackage.IsActive);

        var result = await context.SaveChangesAsync(cancellationToken) > 0;

        return new UpdateServicePackageResult(result);
    }
}
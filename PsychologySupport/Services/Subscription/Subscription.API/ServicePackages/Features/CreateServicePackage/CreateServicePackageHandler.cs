using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using MassTransit;
using Subscription.API.Data;
using Subscription.API.ServicePackages.Dtos;
using Subscription.API.ServicePackages.Models;

namespace Subscription.API.ServicePackages.Features.CreateServicePackage;

public record CreateServicePackageCommand(CreateServicePackageDto ServicePackage) : ICommand<CreateServicePackageResult>;

public record CreateServicePackageResult(Guid Id);

public class CreateServicePackageHandler(SubscriptionDbContext context)
    : ICommandHandler<CreateServicePackageCommand, CreateServicePackageResult>
{
    public async Task<CreateServicePackageResult> Handle(CreateServicePackageCommand request, CancellationToken cancellationToken)
    {
        var dto = request.ServicePackage;

        if (context.ServicePackages.Any(p => p.Name == dto.Name))
            throw new BadRequestException("Service package already exists.");

        var servicePackageId = Guid.NewGuid();
        var servicePackage = ServicePackage.Create(
            servicePackageId,
            dto.Name,
            dto.Description,
            dto.Price,
            dto.DurationDays,
            Guid.Empty,
            true
        );

        context.ServicePackages.Add(servicePackage);
        await context.SaveChangesAsync(cancellationToken);

        return new CreateServicePackageResult(servicePackage.Id);
    }
}

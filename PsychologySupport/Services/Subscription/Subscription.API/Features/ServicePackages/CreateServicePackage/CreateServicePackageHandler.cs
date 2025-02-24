using BuildingBlocks.CQRS;
using Subscription.API.Data;
using Subscription.API.Dtos;
using Subscription.API.Models;

namespace Subscription.API.Features.ServicePackages.CreateServicePackage;

public record CreateServicePackageCommand(
    string Name,
    string Description,
    decimal Price,
    int DurationDays,
    Guid ImageId,
    bool IsActive
) : ICommand<CreateServicePackageResult>;

public record CreateServicePackageResult(Guid Id);

public class CreateServicePackageHandler : ICommandHandler<CreateServicePackageCommand, CreateServicePackageResult>
{
    private readonly SubscriptionDbContext _context;

    public CreateServicePackageHandler(SubscriptionDbContext context)
    {
        _context = context;
    }

    public async Task<CreateServicePackageResult> Handle(CreateServicePackageCommand request, CancellationToken cancellationToken)
    {
        
        var servicePackage = new ServicePackage(
            Guid.NewGuid(),
            request.Name,
            request.Description,
            request.Price,
            request.DurationDays,
            request.ImageId,
            request.IsActive
        );

        _context.ServicePackages.Add(servicePackage);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateServicePackageResult(servicePackage.Id);
    }
}


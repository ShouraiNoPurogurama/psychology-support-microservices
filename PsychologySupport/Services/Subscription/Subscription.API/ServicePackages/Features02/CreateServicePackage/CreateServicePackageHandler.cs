using BuildingBlocks.CQRS;
using Subscription.API.Data;
using Subscription.API.ServicePackages.Models;

namespace Subscription.API.ServicePackages.Features02.CreateServicePackage;

public record CreateServicePackageCommand(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int DurationDays,
    Guid ImageId
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
        var servicePackage = ServicePackage.Create(request.Id, request.Name, request.Description, request.Price, request.DurationDays, request.ImageId, true);

        _context.ServicePackages.Add(servicePackage);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateServicePackageResult(servicePackage.Id);
    }
}
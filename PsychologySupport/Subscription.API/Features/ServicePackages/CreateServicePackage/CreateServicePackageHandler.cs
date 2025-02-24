using BuildingBlocks.CQRS;
using Subscription.API.Data;
using Subscription.API.Models;

namespace Subscription.API.Features.ServicePackages.CreateServicePackage;

public record CreateServicePackageCommand(ServicePackage ServicePackage) : ICommand<CreateServicePackageResult>;

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
        _context.ServicePackages.Add(request.ServicePackage);

        await _context.SaveChangesAsync(cancellationToken);

        return new CreateServicePackageResult(request.ServicePackage.Id);
    }
}

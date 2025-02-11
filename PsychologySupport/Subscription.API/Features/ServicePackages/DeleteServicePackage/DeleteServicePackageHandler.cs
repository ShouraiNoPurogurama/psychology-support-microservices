using BuildingBlocks.CQRS;
using Subscription.API.Data;
using Subscription.API.Exceptions;

namespace Subscription.API.Features.ServicePackages.DeleteServicePackage;

public record DeleteServicePackageCommand(Guid Id) : ICommand<DeleteServicePackageResult>;

public record DeleteServicePackageResult(bool IsSuccess);

public class DeleteServicePackageHandler : ICommandHandler<DeleteServicePackageCommand, DeleteServicePackageResult>
{
    private readonly SubscriptionDbContext _context;

    public DeleteServicePackageHandler(SubscriptionDbContext context)
    {
        _context = context;
    }

    public async Task<DeleteServicePackageResult> Handle(DeleteServicePackageCommand request, CancellationToken cancellationToken)
    {
        var existingServicePackage = await _context.ServicePackages.FindAsync(request.Id)
                                      ?? throw new SubscriptionNotFoundException("Service Package", request.Id);

        _context.ServicePackages.Remove(existingServicePackage);

        var result = await _context.SaveChangesAsync(cancellationToken) > 0;

        return new DeleteServicePackageResult(result);
    }
}

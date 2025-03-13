using BuildingBlocks.CQRS;
using Subscription.API.Data;
using Subscription.API.Data.Common;
using Subscription.API.Exceptions;

namespace Subscription.API.UserSubscriptions.Features.UpdateSubscriptionStatus;

public record UpdateUserSubscriptionStatusCommand(Guid SubscriptionId, SubscriptionStatus Status) : ICommand<UpdateUserSubscriptionStatusResult>;

public record UpdateUserSubscriptionStatusResult(bool IsSuccess);

public class UpdateUserSubscriptionStatusHandler : ICommandHandler<UpdateUserSubscriptionStatusCommand, UpdateUserSubscriptionStatusResult>
{
    private readonly SubscriptionDbContext _context;

    public UpdateUserSubscriptionStatusHandler(SubscriptionDbContext context)
    {
        _context = context;
    }

    public async Task<UpdateUserSubscriptionStatusResult> Handle(UpdateUserSubscriptionStatusCommand request,
        CancellationToken cancellationToken)
    {
        var existingSubscription = await _context.UserSubscriptions.FindAsync(request.SubscriptionId, cancellationToken)
                                   ?? throw new SubscriptionNotFoundException("User Subscription", request.SubscriptionId);

        existingSubscription.Status = request.Status;

        _context.UserSubscriptions.Update(existingSubscription);

        return new UpdateUserSubscriptionStatusResult(await _context.SaveChangesAsync(cancellationToken) > 0);
    }
}
using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using Subscription.API.Data;
using Subscription.API.Data.Common;
using Subscription.API.Exceptions;

namespace Subscription.API.UserSubscriptions.Features02.UpdateSubscriptionStatus;

public record UpdateUserSubscriptionStatusCommand(
    Guid SubscriptionId,
    SubscriptionStatus Status,
    bool DeactivateOldSubscriptions = false) : ICommand<UpdateUserSubscriptionStatusResult>;

public record UpdateUserSubscriptionStatusResult(bool IsSuccess);

public class UpdateUserSubscriptionStatusHandler : ICommandHandler<UpdateUserSubscriptionStatusCommand,
    UpdateUserSubscriptionStatusResult>
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

        if (request.DeactivateOldSubscriptions)
        {
            var oldSubscriptions = await _context.UserSubscriptions
                .Where(x => x.PatientId == existingSubscription.PatientId && x.Status == SubscriptionStatus.Active)
                .ToListAsync(cancellationToken: cancellationToken);


            foreach (var subscription in oldSubscriptions)
            {
                subscription.Status = SubscriptionStatus.Cancelled;
            }
            
            _context.UserSubscriptions.UpdateRange(oldSubscriptions);
        }

        existingSubscription.Status = request.Status;

        _context.UserSubscriptions.Update(existingSubscription);

        await _context.SaveChangesAsync(cancellationToken);

        return new UpdateUserSubscriptionStatusResult(await _context.SaveChangesAsync(cancellationToken) > 0);
    }
}
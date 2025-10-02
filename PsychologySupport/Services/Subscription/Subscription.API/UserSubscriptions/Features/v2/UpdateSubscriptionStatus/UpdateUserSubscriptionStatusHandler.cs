using BuildingBlocks.CQRS;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Subscription;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Subscription.API.Common.Authentication;
using Subscription.API.Data;
using Subscription.API.Data.Common;
using Subscription.API.Exceptions;

namespace Subscription.API.UserSubscriptions.Features.v2.UpdateSubscriptionStatus;

public record UpdateUserSubscriptionStatusCommand(
    Guid SubscriptionId,
    SubscriptionStatus Status,
    bool DeactivateOldSubscriptions = false) : ICommand<UpdateUserSubscriptionStatusResult>;

public record UpdateUserSubscriptionStatusResult(bool IsSuccess);

public class UpdateUserSubscriptionStatusHandler : ICommandHandler<UpdateUserSubscriptionStatusCommand,
    UpdateUserSubscriptionStatusResult>
{
    private readonly SubscriptionDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ICurrentActorAccessor _actorAccessor;

    public UpdateUserSubscriptionStatusHandler(SubscriptionDbContext context, IPublishEndpoint publishEndpoint,
        ICurrentActorAccessor actorAccessor)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
        _actorAccessor = actorAccessor;
    }

    public async Task<UpdateUserSubscriptionStatusResult> Handle(UpdateUserSubscriptionStatusCommand request,
        CancellationToken cancellationToken)
    {
        var existingSubscription = await _context.UserSubscriptions
            .Include(x => x.ServicePackage)
            .FirstOrDefaultAsync(x => x.Id == request.SubscriptionId, cancellationToken)
                               ?? throw new SubscriptionNotFoundException("User Subscription", request.SubscriptionId);

        if (request.DeactivateOldSubscriptions)
        {
            var oldSubscriptions = await _context.UserSubscriptions
                .Where(x => x.PatientId == existingSubscription.PatientId && x.Status == SubscriptionStatus.Active)
                .ToListAsync(cancellationToken);

            foreach (var subscription in oldSubscriptions)
            {
                subscription.Status = SubscriptionStatus.Cancelled;
            }

            _context.UserSubscriptions.UpdateRange(oldSubscriptions);
        }

        existingSubscription.Status = request.Status;
        _context.UserSubscriptions.Update(existingSubscription);

        var result = await _context.SaveChangesAsync(cancellationToken) > 0;

        
        if (existingSubscription.Status == SubscriptionStatus.Active)
        {
            var @event = new UserSubscriptionActivatedIntegrationEvent(
                _actorAccessor.GetRequiredSubjectRef(),
                existingSubscription.ServicePackage.Name
            );
            await _publishEndpoint.Publish(@event, cancellationToken);
        }

        return new UpdateUserSubscriptionStatusResult(result);
    }
}
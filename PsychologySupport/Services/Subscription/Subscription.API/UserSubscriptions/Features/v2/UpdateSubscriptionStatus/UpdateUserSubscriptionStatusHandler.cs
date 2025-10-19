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
    Guid SubjectRef,
    Guid SubscriptionId,
    SubscriptionStatus Status,
    bool DeactivateOldSubscriptions = false) : ICommand<UpdateUserSubscriptionStatusResult>;

public record UpdateUserSubscriptionStatusResult(bool IsSuccess);

public class UpdateUserSubscriptionStatusHandler : ICommandHandler<UpdateUserSubscriptionStatusCommand,
    UpdateUserSubscriptionStatusResult>
{
    private readonly SubscriptionDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public UpdateUserSubscriptionStatusHandler(SubscriptionDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
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
            var userSubscriptionActivatedIntegrationEvent = new UserSubscriptionActivatedIntegrationEvent(
                request.SubjectRef,
                existingSubscription.ServicePackage.Name
            );

            var inventoryCreatedIntegrationEvent = new InventoryCreatedIntegrationEvent(
                request.SubjectRef,
                existingSubscription.StartDate,
                existingSubscription.EndDate
            );


            await _publishEndpoint.Publish(userSubscriptionActivatedIntegrationEvent, cancellationToken);
            await _publishEndpoint.Publish(inventoryCreatedIntegrationEvent, cancellationToken);
        }

        return new UpdateUserSubscriptionStatusResult(result);
    }
}
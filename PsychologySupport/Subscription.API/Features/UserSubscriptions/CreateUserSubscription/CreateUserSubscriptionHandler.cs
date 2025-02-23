using BuildingBlocks.CQRS;
using Mapster;
using MassTransit;
using Subscription.API.Data;
using Subscription.API.Models;
using Subscription.API.Events;
using Microsoft.EntityFrameworkCore;

namespace Subscription.API.Features.UserSubscriptions.CreateUserSubscription;

public record CreateUserSubscriptionCommand(UserSubscription UserSubscription) : ICommand<CreateUserSubscriptionResult>;

public record CreateUserSubscriptionResult(Guid Id);

public class CreateUserSubscriptionHandler : ICommandHandler<CreateUserSubscriptionCommand, CreateUserSubscriptionResult>
{
    private readonly SubscriptionDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateUserSubscriptionHandler(SubscriptionDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<CreateUserSubscriptionResult> Handle(CreateUserSubscriptionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _context.UserSubscriptions.Add(request.UserSubscription);
            await _context.SaveChangesAsync(cancellationToken);

            // Publish event to Payment 
            var subscriptionCreatedEvent = new UserSubscriptionCreatedEvent(
                request.UserSubscription.Id,
                request.UserSubscription.PatientId,
                request.UserSubscription.ServicePackageId,
                request.UserSubscription.StartDate,
                request.UserSubscription.EndDate
            );

            await _publishEndpoint.Publish(subscriptionCreatedEvent, cancellationToken);

            return new CreateUserSubscriptionResult(request.UserSubscription.Id);
        }
        catch (DbUpdateException ex)
        {
            throw new Exception($"Database error: {ex.InnerException?.Message}", ex);
        }
    }
}

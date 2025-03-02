using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.Payment;
using Mapster;
using MassTransit;
using Subscription.API.Data;
using Subscription.API.ServicePackages.Models;
using Subscription.API.UserSubscriptions.Dtos;
using Subscription.API.UserSubscriptions.Models;

namespace Subscription.API.UserSubscriptions.Features.CreateUserSubscription;

public record CreateUserSubscriptionCommand(CreateUserSubscriptionDto UserSubscription) : ICommand<CreateUserSubscriptionResult>;

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

    public async Task<CreateUserSubscriptionResult> Handle(CreateUserSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        var dto = request.UserSubscription;

        ServicePackage servicePackage = await _context.ServicePackages.FindAsync([dto.ServicePackageId], cancellationToken)
                                        ?? throw new NotFoundException(nameof(ServicePackage), dto.ServicePackageId);

        var userSubscription = UserSubscription.Create(dto.PatientId, dto.ServicePackageId, dto.StartDate, dto.EndDate,
            dto.PromotionCodeId, dto.GiftId, servicePackage);

        _context.UserSubscriptions.Add(userSubscription);

        await _context.SaveChangesAsync(cancellationToken);

        var subscriptionCreatedEvent = dto.Adapt<UserSubscriptionCreatedIntegrationEvent>();
        servicePackage.Adapt(subscriptionCreatedEvent);
        subscriptionCreatedEvent.SubscriptionId = userSubscription.Id;

        await _publishEndpoint.Publish(subscriptionCreatedEvent, cancellationToken);

        return new CreateUserSubscriptionResult(userSubscription.Id);
    }
}
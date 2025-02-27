using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.Payment;
using MassTransit;
using Subscription.API.Data;
using Subscription.API.Models;
using Microsoft.EntityFrameworkCore;
using Subscription.API.Dtos;
using Subscription.API.Data.Common;

namespace Subscription.API.Features.UserSubscriptions.CreateUserSubscription;

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
        var userSubscription = new UserSubscription
        {
            Id = Guid.NewGuid(),
            PatientId = request.UserSubscription.PatientId,
            ServicePackageId = request.UserSubscription.ServicePackageId,
            PromoCodeId = request.UserSubscription.PromotionCodeId,
            GiftId = request.UserSubscription.GiftId,
            StartDate = request.UserSubscription.StartDate,
            EndDate = request.UserSubscription.EndDate,
            Status = SubscriptionStatus.Active
        };

        _context.UserSubscriptions.Add(userSubscription);
        await _context.SaveChangesAsync(cancellationToken);

        var servicePackage = await _context.ServicePackages
                                 .Where(sp => sp.Id == request.UserSubscription.ServicePackageId)
                                 .FirstOrDefaultAsync(cancellationToken) ??
                             throw new NotFoundException(nameof(ServicePackage), request.UserSubscription.ServicePackageId);

        // Publish event to Payment 
        var subscriptionCreatedEvent = new UserSubscriptionCreatedIntegrationEvent(
            userSubscription.Id,
            request.UserSubscription.PatientId,
            request.UserSubscription.ServicePackageId,
            servicePackage.Price,
            request.UserSubscription.PromotionCodeId,
            request.UserSubscription.GiftId
        );

        await _publishEndpoint.Publish(subscriptionCreatedEvent, cancellationToken);

        return new CreateUserSubscriptionResult(userSubscription.Id);
    }
}
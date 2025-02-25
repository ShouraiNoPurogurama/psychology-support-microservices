using BuildingBlocks.CQRS;
using MassTransit;
using Subscription.API.Data;
using Subscription.API.Models;
using Subscription.API.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components.Forms;
using Subscription.API.Dtos;
using Subscription.API.Data.Common;

namespace Subscription.API.Features.UserSubscriptions.CreateUserSubscription;

public record CreateUserSubscriptionCommand(UserSubscriptionDto UserSubscription) : ICommand<CreateUserSubscriptionResult>;

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
            var userSubscription = new UserSubscription
            {
                Id = request.UserSubscription.Id,
                PatientId = request.UserSubscription.PatientId,
                ServicePackageId = request.UserSubscription.ServicePackageId,
                StartDate = request.UserSubscription.StartDate,
                EndDate = request.UserSubscription.EndDate,
                Status = request.UserSubscription.Status ?? SubscriptionStatus.Active
            };

            _context.UserSubscriptions.Add(userSubscription);
            await _context.SaveChangesAsync(cancellationToken);

            var servicePackage = await _context.ServicePackages
           .Where(sp => sp.Id == request.UserSubscription.ServicePackageId)
           .FirstOrDefaultAsync(cancellationToken);

            if (servicePackage == null)
            {
                throw new Exception("Service Package not found");
            }

            // Publish event to Payment 
            var subscriptionCreatedEvent = new UserSubscriptionCreatedIntegrationEvent(
                request.UserSubscription.Id,
                request.UserSubscription.PatientId,
                request.UserSubscription.ServicePackageId,
                servicePackage.Price,
                request.UserSubscription.PromotionCodeId,
                request.UserSubscription.GiftId,
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

using BuildingBlocks.Messaging.Events.Subscription;
using MassTransit;
using MediatR;
using Subscription.API.Data.Common;
using Subscription.API.UserSubscriptions.Features.v1.UpdateSubscriptionStatus;

namespace Subscription.API.UserSubscriptions.EventHandlers;

public class UpgradeSubscriptionPaymentSuccessIntegrationEventHandler(ISender sender)
    : IConsumer<UpgradeSubscriptionPaymentSuccessIntegrationEvent>
{
    public async Task Consume(ConsumeContext<UpgradeSubscriptionPaymentSuccessIntegrationEvent> context)
    {
        //Activate new subscription and Deactivate old subscription
        await sender.Send(new UpdateUserSubscriptionStatusCommand(context.Message.SubscriptionId, SubscriptionStatus.Active,
            DeactivateOldSubscriptions: true));
    }
}
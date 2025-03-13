using BuildingBlocks.Messaging.Events.Subscription;
using MassTransit;
using MediatR;
using Subscription.API.Data.Common;
using Subscription.API.UserSubscriptions.Features.UpdateSubscriptionStatus;

namespace Subscription.API.UserSubscriptions.EventHandlers;

public class SubscriptionPaymentSuccessIntegrationEventHandler(ISender sender) : IConsumer<SubscriptionPaymentSuccessIntegrationEvent>
{
    public async Task Consume(ConsumeContext<SubscriptionPaymentSuccessIntegrationEvent> context)
    {
        await sender.Send(new UpdateUserSubscriptionStatusCommand(context.Message.SubscriptionId, SubscriptionStatus.Active));
    }
}
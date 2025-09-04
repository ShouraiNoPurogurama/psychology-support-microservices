using BuildingBlocks.Messaging.Events.IntegrationEvents.Payment;
using MassTransit;
using MediatR;
using Subscription.API.Data.Common;
using Subscription.API.UserSubscriptions.Features.v1.UpdateSubscriptionStatus;

namespace Subscription.API.UserSubscriptions.EventHandlers;

public class SubscriptionPaymentSuccessIntegrationEventHandler(ISender sender) : IConsumer<SubscriptionPaymentSuccessIntegrationEvent>
{
    public async Task Consume(ConsumeContext<SubscriptionPaymentSuccessIntegrationEvent> context)
    {
        await sender.Send(new UpdateUserSubscriptionStatusCommand(context.Message.SubscriptionId, SubscriptionStatus.Active));
    }
}
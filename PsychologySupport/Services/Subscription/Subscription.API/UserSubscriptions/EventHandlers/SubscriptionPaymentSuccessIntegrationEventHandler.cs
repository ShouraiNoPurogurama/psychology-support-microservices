using BuildingBlocks.Messaging.Events.Subscription;
using MassTransit;
using Subscription.API.Data;
using Subscription.API.Data.Common;

namespace Subscription.API.UserSubscriptions.EventHandlers;

public class SubscriptionPaymentSuccessIntegrationEventHandler(SubscriptionDbContext dbContext) : IConsumer<SubscriptionPaymentSuccessIntegrationEvent>
{
    public async Task Consume(ConsumeContext<SubscriptionPaymentSuccessIntegrationEvent> context)
    {
        var subscription = await dbContext.UserSubscriptions.FindAsync(context.Message.SubscriptionId);
        subscription!.Status = SubscriptionStatus.Active;
        await dbContext.SaveChangesAsync();
    }
}
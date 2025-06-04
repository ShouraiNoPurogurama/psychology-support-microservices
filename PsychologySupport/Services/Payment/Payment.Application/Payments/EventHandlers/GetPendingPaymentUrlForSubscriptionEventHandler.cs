using BuildingBlocks.Messaging.Events.Payment;
using MassTransit;
using MediatR;
using Payment.Application.Payments.Queries;

namespace Payment.Application.Payments.EventHandlers;

public class GetPendingPaymentUrlForSubscriptionHandler(ISender sender) : IConsumer<GetPendingPaymentUrlForSubscriptionRequest>
{
    public async Task Consume(ConsumeContext<GetPendingPaymentUrlForSubscriptionRequest> context)
    {
        var query = new GetPaymentUrlForSubscriptionQuery(context.Message.SubscriptionId);
        
        var result = await sender.Send(query);
        
        var response = new GetPaymentUrlForSubscriptionResult(result.Url);
        
        await context.RespondAsync(response);
    }
}
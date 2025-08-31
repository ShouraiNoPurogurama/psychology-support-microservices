using BuildingBlocks.Messaging.Events.Subscription;
using MassTransit;
using MediatR;
using Subscription.API.UserSubscriptions.Features.v1.GetUserSubscriptionForClaims;
using GetUserSubscriptionResponse = BuildingBlocks.Messaging.Events.Subscription.GetUserSubscriptionResponse;

namespace Subscription.API.UserSubscriptions.EventHandlers;

public class GetProfileSubscriptionRequestHandler(ISender sender, ILogger<GetProfileSubscriptionRequestHandler> logger)
    : IConsumer<GetUserSubscriptionRequest>
{
    public async Task Consume(ConsumeContext<GetUserSubscriptionRequest> context)
    {
        logger.LogInformation("Received GetUserSubscriptionRequest for ProfileId: {ProfileId}", context.Message.PatientId);

        var query = new GetUserSubscriptionForClaimsQuery(context.Message.PatientId);
        
        var result = await sender.Send(query);
        
        var response = new GetUserSubscriptionResponse(result.PlanName);

        logger.LogInformation("Returning GetUserSubscriptionResponse for ProfileId: {ProfileId}", context.Message.PatientId);

        await context.RespondAsync(response);
    }
}
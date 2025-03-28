using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Subscription.API.UserSubscriptions.Dtos;

namespace Subscription.API.UserSubscriptions.Features.UpgradeUserSubscription;

public record UpgradeUserSubscriptionRequest(UpgradeUserSubscriptionDto UpgradeUserSubscriptionDto);

public record UpgradeUserSubscriptionResponse(Guid Id, string PaymentUrl);

public class UpgradeUserSubscriptionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("user-subscriptions/upgrade", async ([FromBody] UpgradeUserSubscriptionRequest request, ISender sender) =>
                {
                    var command = request.Adapt<UpgradeUserSubscriptionCommand>();

                    var result = await sender.Send(command);

                    var response = result.Adapt<UpgradeUserSubscriptionResponse>();

                    return Results.Created($"/user-subscriptions/{response.Id}", response);
                }
            )
            .WithName("UpgradeUserSubscription")
            .Produces<UpgradeUserSubscriptionResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Upgrade User Subscription")
            .WithSummary("Upgrade User Subscription");
    }
}
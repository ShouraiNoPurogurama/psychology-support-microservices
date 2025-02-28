using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Subscription.API.Dtos;

namespace Subscription.API.Features.UserSubscriptions.UpdateUserSubscription;

public record UpdateUserSubscriptionRequest(UserSubscriptionDto UserSubscription);

public record UpdateUserSubscriptionResponse(bool IsSuccess);

public class UpdateUserSubscriptionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/user-subscription", async ([FromBody] UpdateUserSubscriptionRequest request, ISender sender) =>
            {
                var command = request.Adapt<UpdateUserSubscriptionCommand>();

                var result = await sender.Send(command);

                var response = result.Adapt<UpdateUserSubscriptionResponse>();

                return Results.Ok(response);
            })
            .WithName("UpdateUserSubscription")
            .Produces<UpdateUserSubscriptionResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Update User Subscription")
            .WithSummary("Update User Subscription");
    }
}
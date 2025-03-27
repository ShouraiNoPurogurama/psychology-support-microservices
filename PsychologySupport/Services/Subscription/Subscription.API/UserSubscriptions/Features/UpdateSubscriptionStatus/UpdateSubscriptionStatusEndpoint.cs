using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Subscription.API.Data.Common;

namespace Subscription.API.UserSubscriptions.Features.UpdateSubscriptionStatus;

public record UpdateUserSubscriptionStatusRequest(Guid SubscriptionId, SubscriptionStatus Status);

public record UpdateUserSubscriptionStatusResponse(bool IsSuccess);

public class UpdateUserSubscriptionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/user-subscription/status", async ([FromBody] UpdateUserSubscriptionStatusRequest request, ISender sender) =>
            {
                var command = request.Adapt<UpdateUserSubscriptionStatusCommand>();

                var result = await sender.Send(command);

                var response = result.Adapt<UpdateUserSubscriptionStatusResponse>();

                return Results.Ok(response);
            })
            .WithName("UpdateUserSubscriptionStatus")
            .Produces<UpdateUserSubscriptionStatusResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Update User Subscription Status")
            .WithSummary("Update User Subscription Status");
    }
}
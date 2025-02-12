using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Subscription.API.Features.UserSubscriptions.DeleteUserSubscription;

public record DeleteUserSubscriptionRequest(Guid Id);

public record DeleteUserSubscriptionResponse(bool IsSuccess);

public class DeleteUserSubscriptionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("user-subscriptions", async ([FromBody] DeleteUserSubscriptionRequest request, ISender sender) =>
        {
            var result = await sender.Send(new DeleteUserSubscriptionCommand(request.Id));

            var response = result.Adapt<DeleteUserSubscriptionResponse>();

            return Results.Ok(response);
        })
        .WithName("DeleteUserSubscription")
        .Produces<DeleteUserSubscriptionResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithDescription("Delete User Subscription")
        .WithSummary("Delete User Subscription");
    }
}

using BuildingBlocks.Exceptions;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Subscription.API.Common;
using Subscription.API.UserSubscriptions.Dtos;

namespace Subscription.API.UserSubscriptions.Features.UpdateUserSubscription;

public record UpdateUserSubscriptionRequest(UserSubscriptionDto UserSubscription);

public record UpdateUserSubscriptionResponse(bool IsSuccess);

public class UpdateUserSubscriptionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/user-subscription", async ([FromBody] UpdateUserSubscriptionRequest request, ISender sender, HttpContext httpContext) =>
            {
                // Authorization check
                if (!AuthorizationHelpers.HasViewAccessToPatientProfile(httpContext.User))
                    throw new ForbiddenException();

                var command = request.Adapt<UpdateUserSubscriptionCommand>();

                var result = await sender.Send(command);

                var response = result.Adapt<UpdateUserSubscriptionResponse>();

                return Results.Ok(response);
            })
            .WithName("UpdateUserSubscription")
            .WithTags("UserSubscriptions")
            .Produces<UpdateUserSubscriptionResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Update User Subscription")
            .WithSummary("Update User Subscription");
    }
}
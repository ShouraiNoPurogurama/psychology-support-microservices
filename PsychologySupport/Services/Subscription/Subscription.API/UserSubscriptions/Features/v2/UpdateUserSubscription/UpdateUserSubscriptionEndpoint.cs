using BuildingBlocks.Exceptions;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Subscription.API.Common;
using Subscription.API.UserSubscriptions.Dtos;

namespace Subscription.API.UserSubscriptions.Features.v2.UpdateUserSubscription;

public record UpdateUserSubscriptionV2Request(UserSubscriptionDto UserSubscription);

public record UpdateUserSubscriptionV2Response(bool IsSuccess);

public class UpdateUserSubscriptionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/v2/user-subscription", async ([FromBody] UpdateUserSubscriptionV2Request request, ISender sender, HttpContext httpContext) =>
            {
                // Authorization check
                if (!AuthorizationHelpers.HasViewAccessToPatientProfile(httpContext.User))
                    throw new ForbiddenException();

                var command = request.Adapt<UpdateUserSubscriptionCommand>();

                var result = await sender.Send(command);

                var response = result.Adapt<UpdateUserSubscriptionV2Response>();

                return Results.Ok(response);
            })
            .WithName("UpdateUserSubscription v2")
            .WithTags("UserSubscriptions Version 2")
            .Produces<UpdateUserSubscriptionV2Response>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Update User Subscription")
            .WithSummary("Update User Subscription");
    }
}
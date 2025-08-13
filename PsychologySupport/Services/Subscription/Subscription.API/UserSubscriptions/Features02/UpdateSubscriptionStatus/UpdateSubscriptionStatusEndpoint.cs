using BuildingBlocks.Exceptions;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Subscription.API.Common;
using Subscription.API.Data.Common;

namespace Subscription.API.UserSubscriptions.Features02.UpdateSubscriptionStatus;

public record UpdateUserSubscriptionStatusV2Request(Guid SubscriptionId, SubscriptionStatus Status);

public record UpdateUserSubscriptionStatusV2Response(bool IsSuccess);

public class UpdateUserSubscriptionV2Endpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/v2/user-subscription/status", async ([FromBody] UpdateUserSubscriptionStatusV2Request request, ISender sender, HttpContext httpContext) =>
            {
                // Authorization check
                if (!AuthorizationHelpers.HasViewAccessToPatientProfile(httpContext.User))
                    throw new ForbiddenException();

                var command = request.Adapt<UpdateUserSubscriptionStatusCommand>();

                var result = await sender.Send(command);

                var response = result.Adapt<UpdateUserSubscriptionStatusV2Response>();

                return Results.Ok(response);
            })
            .WithName("UpdateUserSubscriptionStatus v2")
            .WithTags("UserSubscriptions Version 2")
            .Produces<UpdateUserSubscriptionStatusV2Response>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Update User Subscription Status")
            .WithSummary("Update User Subscription Status");
    }
}
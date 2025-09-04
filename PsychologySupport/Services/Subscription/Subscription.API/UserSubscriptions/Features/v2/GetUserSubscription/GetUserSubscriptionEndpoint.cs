using BuildingBlocks.Exceptions;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Subscription.API.Common;
using Subscription.API.UserSubscriptions.Dtos;

namespace Subscription.API.UserSubscriptions.Features.v2.GetUserSubscription;

public record GetUserSubscriptionV2Response(GetUserSubscriptionDto UserSubscription);

public class GetUserSubscriptionV2Endpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v2/user-subscription/{id}", async ([FromRoute]Guid id, ISender sender, HttpContext httpContext) =>
            {
                // Authorization check
                if (!AuthorizationHelpers.HasViewAccessToPatientProfile(httpContext.User))
                    throw new ForbiddenException();

                var query = new GetUserSubscriptionQuery(id);

                var result = await sender.Send(query);

                var response = result.Adapt<GetUserSubscriptionV2Response>();

                return Results.Ok(response);
            })
            .WithName("GetUserSubscription v2")
            .WithTags("UserSubscriptions Version 2")
            .Produces<GetUserSubscriptionV2Response>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get User Subscription")
            .WithSummary("Get User Subscription");
    }
}
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Subscription.API.Common;
using Subscription.API.UserSubscriptions.Dtos;

namespace Subscription.API.UserSubscriptions.Features.GetUserSubscription;

public record GetUserSubscriptionResponse(GetUserSubscriptionDto UserSubscription);

public class GetUserSubscriptionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/user-subscriptions/{id}", async ([FromRoute]Guid id, ISender sender, HttpContext httpContext) =>
            {
                // Authorization check
                if (!AuthorizationHelpers.HasViewAccessToPatientProfile(httpContext.User))
                    return Results.Problem(
                               statusCode: StatusCodes.Status403Forbidden,
                               title: "Forbidden",
                               detail: "You do not have permission to access this resource."
                           );

                var query = new GetUserSubscriptionQuery(id);

                var result = await sender.Send(query);

                var response = result.Adapt<GetUserSubscriptionResponse>();

                return Results.Ok(response);
            })
            .WithName("GetUserSubscription")
            .WithTags("UserSubscriptions")
            .Produces<GetUserSubscriptionResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get User Subscription")
            .WithSummary("Get User Subscription");
    }
}
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Subscription.API.Models;

namespace Subscription.API.Features.UserSubscriptions.GetUserSubscriptions;
public record GetUserSubscriptionsRequest(int PageNumber, int PageSize);

public record GetUserSubscriptionsResponse(IEnumerable<UserSubscription> UserSubscriptions, int TotalCount);

public class GetUserSubscriptionsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/user-subscriptions", async ([FromQuery] int pageNumber, [FromQuery] int pageSize, ISender sender) =>
        {
            var query = new GetUserSubscriptionsQuery(pageNumber, pageSize);
            var result = await sender.Send(query);
            var response = result.Adapt<GetUserSubscriptionsResponse>();
            return Results.Ok(response);
        })
        .WithName("GetUserSubscriptions")
        .Produces<GetUserSubscriptionsResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithDescription("Get User Subscriptions")
        .WithSummary("Get User Subscriptions");
    }
}
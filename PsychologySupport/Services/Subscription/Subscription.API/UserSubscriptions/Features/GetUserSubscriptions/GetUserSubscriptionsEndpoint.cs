using BuildingBlocks.Pagination;
using Carter;
using Mapster;
using MediatR;
using Subscription.API.UserSubscriptions.Dtos;

namespace Subscription.API.UserSubscriptions.Features.GetUserSubscriptions;

public record GetUserSubscriptionsRequest(PaginationRequest PaginationRequest);

public record GetUserSubscriptionsResponse(PaginatedResult<GetUserSubscriptionDto> UserSubscriptions);

public class GetUserSubscriptionsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/user-subscriptions", async (
            [AsParameters] GetUserSubscriptionsQuery request,
            ISender sender) =>
        {
            var result = await sender.Send(request);
            var response = result.Adapt<GetUserSubscriptionsResult>();

            return Results.Ok(response);
        })
        .WithName("GetUserSubscriptions")
        .Produces<GetUserSubscriptionsResult>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithDescription("Get All User Subscriptions")
        .WithSummary("Get All User Subscriptions");
    }
}
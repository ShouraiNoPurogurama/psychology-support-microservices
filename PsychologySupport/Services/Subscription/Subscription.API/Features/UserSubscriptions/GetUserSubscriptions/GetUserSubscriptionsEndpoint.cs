using BuildingBlocks.Pagination;
using Carter;
using Mapster;
using MediatR;
using Subscription.API.Dtos;

namespace Subscription.API.Features.UserSubscriptions.GetUserSubscriptions;

public record GetUserSubscriptionsRequest(PaginationRequest PaginationRequest);

public record GetUserSubscriptionsResponse(PaginatedResult<GetUserSubscriptionDto> UserSubscriptions);

public class GetUserSubscriptionsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/user-subscriptions", async ([AsParameters] PaginationRequest request, ISender sender) =>
            {
                var query = new GetUserSubscriptionsQuery(request);

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
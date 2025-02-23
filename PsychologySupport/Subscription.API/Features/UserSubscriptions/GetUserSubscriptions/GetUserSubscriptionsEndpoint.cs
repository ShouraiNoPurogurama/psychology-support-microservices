using BuildingBlocks.Pagination;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Subscription.API.Dtos;

namespace Subscription.API.Features.UserSubscriptions.GetUserSubscriptions;

public record GetUserSubscriptionsRequest(PaginationRequest PaginationRequest);

public record GetUserSubscriptionsResponse(IEnumerable<GetUserSubscriptionDto> UserSubscriptions, int TotalCount);

public class GetUserSubscriptionsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/user-subscriptions", async ([FromQuery] int pageIndex, [FromQuery] int pageSize, ISender sender) =>
        {
            var paginationRequest = new PaginationRequest(pageIndex, pageSize);
            var query = new GetUserSubscriptionsQuery(paginationRequest);

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

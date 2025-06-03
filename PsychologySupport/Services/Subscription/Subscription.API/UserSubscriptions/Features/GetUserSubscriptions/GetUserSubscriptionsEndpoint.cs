using BuildingBlocks.Pagination;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Subscription.API.Data.Common;
using Subscription.API.UserSubscriptions.Dtos;

namespace Subscription.API.UserSubscriptions.Features.GetUserSubscriptions;

public record GetUserSubscriptionsRequest( [FromQuery] int PageIndex = 1,
    [FromQuery] int PageSize = 10,
    [FromQuery] string? Search = "",
    [FromQuery] string? SortBy = "StartDate",
    [FromQuery] string? SortOrder = "asc",
    [FromQuery] Guid? ServicePackageId = null,
    [FromQuery] Guid? PatientId = null, 
    [FromQuery] SubscriptionStatus? Status = null);

public record GetUserSubscriptionsResponse(PaginatedResult<GetUserSubscriptionDto> UserSubscriptions);

public class GetUserSubscriptionsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/user-subscriptions", async (
                [AsParameters] GetUserSubscriptionsRequest request,
                ISender sender) =>
            {
                var query = request.Adapt<GetUserSubscriptionsQuery>();
                var result = await sender.Send(query);
                var response = result.Adapt<GetUserSubscriptionsResult>();

                return Results.Ok(response);
            })
            .WithName("GetUserSubscriptions")
            .WithTags("UserSubscriptions")
            .Produces<GetUserSubscriptionsResult>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get All User Subscriptions")
            .WithSummary("Get All User Subscriptions");
    }
}
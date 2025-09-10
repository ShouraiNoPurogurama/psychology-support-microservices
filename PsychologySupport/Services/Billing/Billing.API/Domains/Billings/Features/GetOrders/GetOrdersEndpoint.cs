using Billing.API.Domains.Billings.Dtos;
using BuildingBlocks.Pagination;
using Carter;
using Mapster;
using MediatR;

namespace Billing.API.Domains.Billings.Features.GetOrders;

public record GetOrdersRequest(
    int PageIndex = 1,
    int PageSize = 10,
    string? Search = "",
    string? SortBy = "CreatedAt",
    string? SortOrder = "desc",
    string? OrderType = null,
    string? Status = null
);

public record GetOrdersResponse(PaginatedResult<OrderDto> Orders);

public class GetOrdersEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("v1/me/orders", async (
            [AsParameters] GetOrdersRequest request,
            ISender sender) =>
        {
            var query = request.Adapt<GetOrdersQuery>();
            var result = await sender.Send(query);
            var response = result.Adapt<GetOrdersResponse>();

            return Results.Ok(response);
        })
        .WithName("GetOrders")
        .WithTags("Orders")
        .Produces<GetOrdersResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithDescription("GetOrders")
        .WithSummary("GetOrders");
    }
}

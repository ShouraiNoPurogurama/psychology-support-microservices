using BuildingBlocks.Pagination;
using Carter;
using MediatR;
using Post.Application.Features.Gifts.Dtos;
using Post.Application.Features.Gifts.Queries;

namespace Post.API.Endpoints.Gifts;

public sealed record GetGiftsByPostRequest(
    int Page = 1,
    int PageSize = 20,
    string SortBy = "CreatedAt",
    bool SortDescending = true
);

public sealed record GetGiftsByPostResponse(
    PaginatedResult<GiftAttachDto> Gifts
);

public class GetGiftsByPostEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/gifts/post/{postId:guid}", async (
                Guid postId,
                [AsParameters] GetGiftsByPostRequest request,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetGiftsByPostQuery(
                    postId,
                    request.Page,
                    request.PageSize,
                    request.SortBy,
                    request.SortDescending
                );

                var result = await sender.Send(query, ct);

                var response = new GetGiftsByPostResponse(result.Gifts);

                return Results.Ok(response);
            })
            .AllowAnonymous()
            .WithTags("Gifts")
            .WithName("GetGiftsByPost")
            .WithSummary("Retrieves paginated gifts for a post.")
            .WithDescription("This endpoint returns a paginated list of gifts attached to the specified post. Supports sorting and pagination. Accessible to all users.")
            .Produces<GetGiftsByPostResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
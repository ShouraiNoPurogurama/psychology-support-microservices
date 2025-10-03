using BuildingBlocks.Pagination;
using Carter;
using Mapster;
using MediatR;
using Post.Application.Features.Reactions.Dtos;
using Post.Application.Features.Reactions.Queries.GetReactionsForPost;

namespace Post.API.Endpoints.Reactions;

public sealed record GetReactionsForPostResponse(
    PaginatedResult<ReactionDto> Reactions
);

public class GetReactionsForPostEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/posts/{postId:guid}/reactions", async (
                Guid postId,
                [AsParameters] PaginationRequest pagination,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetReactionsForPostQuery(
                    postId,
                    pagination.PageIndex,
                    pagination.PageSize
                );

                var result = await sender.Send(query, ct);

                var response = result.Adapt<GetReactionsForPostResponse>();

                return Results.Ok(response);
            })
            .WithTags("Reactions")
            .WithName("GetReactionsForPost")
            .WithSummary("Retrieves paginated reactions for a specific post.")
            .WithDescription("This endpoint returns a paginated list of reactions for the specified post. Supports pagination parameters. Results include reaction details and counts. Accessible to all users.")
            .Produces<GetReactionsForPostResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}

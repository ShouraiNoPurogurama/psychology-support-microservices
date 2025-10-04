using Carter;
using Mapster;
using MediatR;
using Post.Application.Features.CategoryTags.Dtos;
using Post.Application.Features.CategoryTags.Queries.GetAllCategoryTags;

namespace Post.API.Endpoints.CategoryTags;

public sealed record GetAllCategoryTagsResponse(
    IEnumerable<CategoryTagDetailDto> CategoryTags
);

public class GetAllCategoryTagsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/category-tags", async (
                bool? isActive,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetAllCategoryTagsQuery(isActive);

                var result = await sender.Send(query, ct);

                var response = result.Adapt<GetAllCategoryTagsResponse>();

                return Results.Ok(response);
            })
            .WithTags("CategoryTags")
            .WithName("GetAllCategoryTags")
            .WithSummary("Retrieves all category tags.")
            .WithDescription("This endpoint returns all category tags available in the system. Optionally filter by active status using the 'isActive' query parameter. Returns tags ordered by sort order and display name.")
            .Produces<GetAllCategoryTagsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}

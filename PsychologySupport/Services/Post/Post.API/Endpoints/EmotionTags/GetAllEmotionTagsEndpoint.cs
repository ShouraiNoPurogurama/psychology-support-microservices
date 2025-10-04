using Carter;
using Mapster;
using MediatR;
using Post.Application.Features.EmotionTags.Dtos;
using Post.Application.Features.EmotionTags.Queries.GetAllEmotionTags;

namespace Post.API.Endpoints.EmotionTags;

public sealed record GetAllEmotionTagsResponse(
    IEnumerable<EmotionTagDto> EmotionTags
);

public class GetAllEmotionTagsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/emotion-tags", async (
                Guid? aliasId,
                bool? isActive,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetAllEmotionTagsQuery(aliasId, isActive);

                var result = await sender.Send(query, ct);

                var response = result.Adapt<GetAllEmotionTagsResponse>();

                return Results.Ok(response);
            })
            .WithTags("EmotionTags")
            .WithName("GetAllEmotionTags")
            .WithSummary("Retrieves all emotion tags with ownership information.")
            .WithDescription("This endpoint returns all emotion tags available in the system. If 'aliasId' is provided, the response includes ownership information for that user. Optionally filter by active status using the 'isActive' query parameter. Returns tags ordered by display name.")
            .Produces<GetAllEmotionTagsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}

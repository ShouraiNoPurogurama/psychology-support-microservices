using Carter;
using DigitalGoods.API.Dtos;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace DigitalGoods.API.Features.EmotionTags.GetEmotionTag;

public record GetEmotionTagsByTopicRequest(
    Guid SubjectRef
);

public record GetEmotionTagsByTopicResponse(
    Dictionary<string, List<EmotionTagDto>> GroupedTags
);

public class GetEmotionTagEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/emotion-tags", async (
                [AsParameters] GetEmotionTagsByTopicRequest request,
                ISender sender,
                HttpContext httpContext) =>
        {

            var query = new GetEmotionTagsByTopicQuery(request.SubjectRef);
            var result = await sender.Send(query);

            return Results.Ok(new GetEmotionTagsByTopicResponse(result.GroupedByTopic));
        })
        .WithName("GetEmotionTagsByTopic")
        .WithTags("EmotionTags")
        .Produces<GetEmotionTagsByTopicResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Get EmotionTags grouped by topic")
        .WithDescription("Returns all EmotionTags grouped by Topic and ownership state.");
    }
}

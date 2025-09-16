using Post.Application.Aggregates.Posts.Queries.GenerateAIPrompts;
using Carter;
using Mapster;
using MediatR;

namespace Post.API.Endpoints.Posts;

public sealed record GenerateAIPromptsRequest(
    Guid? CategoryTagId,
    int Count
);

public sealed record GenerateAIPromptsResponse(
    List<AIPromptDto> Prompts
);

public class GenerateAIPromptsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/posts/ai-prompts", async (
                [AsParameters] GenerateAIPromptsRequest request,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new GenerateAIPromptsQuery(
                    request.CategoryTagId,
                    request.Count <= 0 ? 5 : Math.Min(request.Count, 20) // Limit to max 20 prompts
                );

                var result = await sender.Send(query, ct);

                var response = result.Adapt<GenerateAIPromptsResponse>();

                return Results.Ok(response);
            })
            .WithTags("Posts")
            .WithName("GenerateAIPrompts")
            .WithSummary("Generate AI-powered writing prompts for posts")
            .Produces<GenerateAIPromptsResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem();
    }
}

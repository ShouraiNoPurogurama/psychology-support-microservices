using BuildingBlocks.CQRS;
using Post.Application.Features.Posts.Dtos;

namespace Post.Application.Features.Posts.Queries.GenerateAIPrompts;

public record GenerateAIPromptsQuery(
    Guid? CategoryTagId = null,
    int Count = 5
) : IQuery<GenerateAIPromptsResult>;

public record GenerateAIPromptsResult(
    List<AIPromptDto> Prompts
);

using BuildingBlocks.CQRS;

namespace Post.Application.Features.Posts.Queries.GenerateAIPrompts;

public record GenerateAIPromptsQuery(
    Guid? CategoryTagId = null,
    int Count = 5
) : IQuery<GenerateAIPromptsResult>;

public record GenerateAIPromptsResult(
    List<AIPromptDto> Prompts
);

public record AIPromptDto(
    string Title,
    string Description,
    List<string> SuggestedTags,
    string PromptType // "question", "topic", "challenge"
);

namespace Post.Application.Features.Posts.Dtos;

public record AIPromptDto(
    string Title,
    string Description,
    List<string> SuggestedTags,
    string PromptType // "question", "topic", "challenge"
);

namespace Post.Application.Features.Reactions.Dtos;

public record ReactionDto(
    Guid Id,
    Guid PostId,
    Guid AliasId,
    string AliasDisplayName,
    string ReactionCode,
    DateTimeOffset CreatedAt
);

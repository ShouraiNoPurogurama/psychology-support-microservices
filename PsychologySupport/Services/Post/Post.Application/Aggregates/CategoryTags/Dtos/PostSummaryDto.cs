namespace Post.Application.Aggregates.CategoryTags.Queries.GetPostsByCategoryTag;

public record PostSummaryDto(
    Guid Id,
    string? Title,
    string Content,
    Guid AuthorAliasId,
    string Visibility,
    DateTimeOffset PublishedAt,
    DateTimeOffset? EditedAt,
    int ReactionCount,
    int CommentCount,
    int ViewCount
);
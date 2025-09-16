using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Post.Domain.Aggregates.Posts.Enums;

namespace Post.Application.Aggregates.Posts.Queries.GetPosts;

public record GetPostsQuery(
    int PageIndex = 1,
    int PageSize = 20,
    string? Visibility = null,
    List<Guid>? CategoryTagIds = null,
    string? SortBy = "CreatedAt",
    bool SortDescending = true
) : IQuery<PaginatedResult<PostSummaryDto>>;

public record PostSummaryDto(
    Guid Id,
    string Content,
    string? Title,
    PostVisibility Visibility,
    AuthorSummaryDto Author,
    ModerationStatus ModerationStatus,
    MetricsSummaryDto Metrics,
    int MediaCount,
    List<string> CategoryCodes,
    DateTimeOffset CreatedAt
);

public record AuthorSummaryDto(
    Guid AliasId,
    Guid? AliasVersionId
);

public record MetricsSummaryDto(
    int ReactionCount,
    int CommentCount,
    int ViewCount
);

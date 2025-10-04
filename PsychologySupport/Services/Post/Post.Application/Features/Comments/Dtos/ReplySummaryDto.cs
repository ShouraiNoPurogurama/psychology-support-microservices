using Post.Application.Features.Posts.Dtos;

namespace Post.Application.Features.Comments.Dtos;

public record ReplySummaryDto(
    Guid Id,
    Guid PostId,
    string Content,
    bool IsReactedByCurrentUser,
    AuthorDto Author,
    HierarchyDto Hierarchy,
    DateTimeOffset CreatedAt,
    DateTimeOffset? EditedAt,
    int ReactionCount,
    int ReplyCount,
    bool IsDeleted
);

using Post.Application.Aggregates.Comments.Queries.GetComments;
using Post.Application.Aggregates.Posts.Dtos;

namespace Post.Application.Aggregates.Comments.Dtos;

public record CommentDto(
    Guid Id,
    Guid PostId,
    string Content,
    AuthorDto Author,
    HierarchyDto Hierarchy,
    DateTimeOffset CreatedAt,
    DateTimeOffset? EditedAt,
    int ReactionCount,
    int ReplyCount,
    bool IsDeleted
);
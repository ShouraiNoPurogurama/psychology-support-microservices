using Post.Application.Features.Comments.Queries.GetComments;
using Post.Application.Features.Posts.Dtos;

namespace Post.Application.Features.Comments.Dtos;

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
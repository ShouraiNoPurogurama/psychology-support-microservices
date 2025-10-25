using Post.Application.Features.Gifts.Dtos;
using Post.Application.Features.Posts.Commands.CreatePost;
using Post.Domain.Aggregates.Posts.Enums;

namespace Post.Application.Features.Posts.Dtos;

public record PostSummaryDto(
    Guid Id,
    string? Title,
    string Content,
    bool IsReactedByCurrentUser,
    AuthorDto Author,
    PostVisibility Visibility,
    DateTimeOffset PublishedAt,
    DateTimeOffset? EditedAt,
    int ReactionCount,
    int CommentCount,
    int ViewCount,
    int GiftCount,
    bool HasMedia,
    IReadOnlyList<MediaItemDto> Medias,
    IReadOnlyList<Guid> CategoryTagIds,
    IReadOnlyList<Guid> EmotionTagIds
);


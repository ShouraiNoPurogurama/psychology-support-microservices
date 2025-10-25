using Post.Application.Features.Posts.Dtos;

namespace Post.Application.Features.Gifts.Dtos;

public record GiftAttachDto(
    Guid GiftId,
    Guid PostId,
    string? Message,
    AuthorDto Author,
    DateTimeOffset CreatedAt,
    bool IsDeleted
);
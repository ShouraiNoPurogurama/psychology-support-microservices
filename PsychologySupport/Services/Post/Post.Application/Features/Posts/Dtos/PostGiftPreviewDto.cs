namespace Post.Application.Features.Posts.Dtos;

public record PostGiftPreviewDto(
    Guid Id,
    string? Message,
    AuthorDto Author,
    DateTimeOffset CreatedAt
);
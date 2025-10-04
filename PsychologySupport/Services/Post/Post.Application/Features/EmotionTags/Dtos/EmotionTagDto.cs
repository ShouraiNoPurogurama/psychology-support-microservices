namespace Post.Application.Features.EmotionTags.Dtos;

public record EmotionTagDto(
    Guid Id,
    string Code,
    string DisplayName,
    Guid? MediaId,
    bool IsActive,
    bool IsOwnedByUser,
    string? UnicodeCodepoint
);

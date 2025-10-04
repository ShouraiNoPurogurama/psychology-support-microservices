namespace Post.Application.Features.CategoryTags.Dtos;

public record CategoryTagDetailDto(
    Guid Id,
    string Code,
    string DisplayName,
    string? Color,
    string? UnicodeCodepoint,
    bool IsActive,
    int SortOrder
);

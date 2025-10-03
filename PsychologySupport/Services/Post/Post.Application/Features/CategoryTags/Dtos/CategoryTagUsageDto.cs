namespace Post.Application.Features.CategoryTags.Dtos;

public record CategoryTagUsageDto(
    Guid Id,
    string DisplayName,
    string Color,
    int UsageCount,
    DateTimeOffset LastUsed
);

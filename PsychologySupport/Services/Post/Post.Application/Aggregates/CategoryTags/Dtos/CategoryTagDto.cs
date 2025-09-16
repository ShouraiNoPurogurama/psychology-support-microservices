namespace Post.Application.Aggregates.CategoryTags.Dtos;

public record CategoryTagDto(
    Guid Id,
    string Name,
    string? Description,
    DateTimeOffset AssignedAt
);
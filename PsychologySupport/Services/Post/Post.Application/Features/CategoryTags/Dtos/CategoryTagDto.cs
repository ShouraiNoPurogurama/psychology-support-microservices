namespace Post.Application.Features.CategoryTags.Dtos;

public record CategoryTagDto(
    Guid Id,
    string Name,
    string? Description,
    DateTimeOffset AssignedAt
);
namespace Post.Application.Aggregates.CategoryTags.Queries.GetCategoryTagsByPost;

public record CategoryTagDto(
    Guid Id,
    string Name,
    string? Description,
    DateTimeOffset AssignedAt
);
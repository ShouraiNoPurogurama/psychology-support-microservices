namespace Feed.Application.Dtos;

public record PostInfo(Guid PostId, Guid AuthorAliasId, DateTimeOffset CreatedAt);

namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;

public record CommentCreatedIntegrationEvent(
    Guid CommentId,
    Guid PostId,
    Guid PostAuthorAliasId,
    Guid CommentAuthorAliasId,
    string CommentAuthorLabel,
    Guid? ParentCommentId,
    Guid? ParentCommentAuthorAliasId,
    string CommentSnippet,
    DateTimeOffset CreatedAt
) : IntegrationEvent;


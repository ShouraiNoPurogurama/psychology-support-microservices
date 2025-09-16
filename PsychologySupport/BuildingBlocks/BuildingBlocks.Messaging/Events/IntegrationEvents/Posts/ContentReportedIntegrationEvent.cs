namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;

public record ContentReportedIntegrationEvent(
    Guid ReportId,
    string ContentType, // POST, COMMENT
    Guid ContentId,
    Guid ContentAuthorAliasId,
    Guid ReporterAliasId,
    string Reason,
    DateTimeOffset ReportedAt
) : IntegrationEvent;
